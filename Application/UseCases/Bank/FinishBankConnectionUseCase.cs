using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;
using Domain.Bank.Enums;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.Sessions;

namespace Application.UseCases.Bank
{
    public interface IFinishBankConnectionUseCase
    {
        Task<CaseResult<string>> InvokeAsync(Guid sessionId, string code);
    }

    public class FinishBankConnectionUseCase : IFinishBankConnectionUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private ISessionsService _SessionsService { get; set; }

        public FinishBankConnectionUseCase(IUnitOfWork unitOfWork, IUserService userService, ISessionsService sessionsService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _SessionsService = sessionsService;
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
                    Console.WriteLine($"Cant find Bank Consent with id: {sessionId}.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find initiated bank connection. Please try again.";
                    return result;
                }

                ApiResponse<AuthorizeSessionResponse> authSessionResponse = await _SessionsService.AuthorizeSessionAsync(new AuthorizeSessionRequest() { Code = code }, new CancellationToken());

                if (authSessionResponse.StatusCode != System.Net.HttpStatusCode.OK || authSessionResponse.Data == null)
                {
                    Console.WriteLine(authSessionResponse.Error?.Detail);
                    result.Successful = false;
                    result.ErrorMessage = authSessionResponse.Error?.Message;
                    return result;
                }

                if (authSessionResponse.StatusCode != System.Net.HttpStatusCode.OK || authSessionResponse.Data == null || authSessionResponse.Data.Accounts == null)
                {
                    Console.WriteLine(authSessionResponse.Error?.Detail);
                    bankConsent.State = BankAccountStaatus.ConnectionFailed;
                    await _UnitOfWork.BankConsentRepository.Update(bankConsent);
                    result.Successful = false;
                    result.Data = "monthspendings://(main)/ConnectBankError";
                    result.ErrorMessage = authSessionResponse.Error?.Message;
                    return result;
                }

                bankConsent.State = BankAccountStaatus.Connected;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during finish bank connection. Please try again later.";
            }
            return result;
        }
    }
}
