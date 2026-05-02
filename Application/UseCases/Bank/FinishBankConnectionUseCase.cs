using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;
using Domain.Bank.Enums;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.Sessions;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface IFinishBankConnectionUseCase
    {
        Task<CaseResult<string>> InvokeAsync(Guid sessionId, string code);
    }

    public class FinishBankConnectionUseCase : IFinishBankConnectionUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private ISessionsService _SessionsService { get; set; }
        private readonly ILogger<FinishBankConnectionUseCase> _Logger;

        public FinishBankConnectionUseCase(IUnitOfWork unitOfWork, ISessionsService sessionsService, ILogger<FinishBankConnectionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _SessionsService = sessionsService;
            _Logger = logger;
        }

        public async Task<CaseResult<string>> InvokeAsync(Guid sessionId, string code)
        {
            var result = new CaseResult<string>();
            result.Successful = true;

            try
            {
                BankConsent? bankConsent = await _UnitOfWork.BankConsentRepository.GetBankConsentBySessionId(sessionId);

                if (bankConsent == null)
                {
                    _Logger.LogWarning("BankConsent not found for sessionId {SessionId}", sessionId);
                    result.Successful = false;
                    result.Data = "monthspendings://(main)/ConnectBankFail";
                    result.ErrorMessage = "Can't find initiated bank connection. Please try again.";
                    return result;
                }

                ApiResponse<AuthorizeSessionResponse> authSessionResponse = await _SessionsService.AuthorizeSessionAsync(new AuthorizeSessionRequest() { Code = code }, new CancellationToken());

                if (authSessionResponse.StatusCode != System.Net.HttpStatusCode.OK || authSessionResponse.Data == null)
                {
                    _Logger.LogError("EnableBanking AuthorizeSession failed: {ErrorDetail}", authSessionResponse.Error?.Detail);
                    result.Successful = false;
                    result.Data = "monthspendings://(main)/ConnectBankFail";
                    result.ErrorMessage = authSessionResponse.Error?.Message;
                    return result;
                }

                if (authSessionResponse.Data.Accounts != null && authSessionResponse.Data.Accounts.All(account => account.Uid == null))
                {
                    _Logger.LogError("All accounts returned from EnableBanking have null AccountId for sessionId {SessionId}", sessionId);
                    result.Successful = false;
                    result.Data = "monthspendings://(main)/ConnectBankFail";
                    result.ErrorMessage = authSessionResponse.Error?.Message;
                    return result;
                }

                if (authSessionResponse.StatusCode != System.Net.HttpStatusCode.OK || authSessionResponse.Data == null || authSessionResponse.Data.Accounts == null)
                {
                    _Logger.LogError("EnableBanking account details failed: {ErrorDetail}", authSessionResponse.Error?.Detail);
                    bankConsent.State = BankAccountStatus.ConnectionFailed;
                    await _UnitOfWork.BankConsentRepository.Update(bankConsent);
                    result.Successful = false;
                    result.Data = "monthspendings://(main)/ConnectBankError";
                    result.ErrorMessage = authSessionResponse.Error?.Message;
                    return result;
                }

                bankConsent.State = BankAccountStatus.Connected;
                bankConsent.Accounts = authSessionResponse.Data.Accounts.Where(acc => acc.Uid.HasValue).Select(ba => new BankAccount()
                {
                    AccountUuid = ba.Uid!.Value,
                    ConsentId = bankConsent.Id,
                    Currency = ba.Currency ?? "not available",
                    HolderName = ba.Name ?? "not available",
                    Iban = ba.AccountId?.Iban ?? "not available"
                }
                ).ToList();

                await _UnitOfWork.BankConsentRepository.Update(bankConsent);
                await _UnitOfWork.CommitAsync();
                result.Data = "monthspendings://(main)/ConnectBankSuccess";
                _Logger.LogInformation("Bank connection finished for sessionId {SessionId}: {AccountCount} accounts linked", sessionId, bankConsent.Accounts.Count);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error finishing bank connection for sessionId {SessionId}", sessionId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during finish bank connection. Please try again later.";
            }
            return result;
        }
    }
}
