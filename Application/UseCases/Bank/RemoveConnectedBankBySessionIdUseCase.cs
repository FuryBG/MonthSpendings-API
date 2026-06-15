using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;
using EnableBanking.Interfaces;
using EnableBanking.Models.Sessions;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface IRemoveConnectedBankBySessionIdUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(Guid sessionId, CancellationToken cancellationToken);
    }

    public class RemoveConnectedBankBySessionIdUseCase : IRemoveConnectedBankBySessionIdUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private ISessionsService _SessionsService { get; set; }
        private readonly ILogger<RemoveConnectedBankBySessionIdUseCase> _Logger;

        public RemoveConnectedBankBySessionIdUseCase(IUnitOfWork unitOfWork, IUserService userService, ISessionsService sessionsService, ILogger<RemoveConnectedBankBySessionIdUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _SessionsService = sessionsService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                BankConsent? bankConsent = await _UnitOfWork.BankConsentRepository.GetBankConsentBySessionId(sessionId);

                if (bankConsent == null || bankConsent.UserId != userId)
                {
                    _Logger.LogWarning("BankConsent with sessionId {SessionId} not found for user {UserId}", sessionId, userId);
                    result.Successful = false;
                    result.ErrorMessage = "Something got wrong during remove bank account. Please try again later.";
                    return result;
                }

                if (bankConsent.EnableBankingSessionId.HasValue)
                {
                    var deleteSessionResponse = await _SessionsService.DeleteSessionAsync(new DeleteSessionRequest { SessionId = bankConsent.EnableBankingSessionId }, cancellationToken);
                    if (deleteSessionResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _Logger.LogWarning("Failed to revoke EnableBanking session {EnableBankingSessionId} for sessionId {SessionId}: {Error}", bankConsent.EnableBankingSessionId, sessionId, deleteSessionResponse.Error?.Detail);
                    }
                }

                int deleted = await _UnitOfWork.BankConsentRepository.Delete(((BankConsent bc) =>
                    bc.SessionId == sessionId &&
                    bc.UserId == userId), cancellationToken);

                if (deleted <= 0)
                {
                    _Logger.LogWarning("BankConsent with sessionId {SessionId} not found for user {UserId}", sessionId, userId);
                    result.Successful = false;
                    result.ErrorMessage = "Something got wrong during remove bank account. Please try again later.";
                    return result;
                }

                _Logger.LogInformation("Bank connection {SessionId} removed for user {UserId}", sessionId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error removing bank connection {SessionId} for user {UserId}", sessionId, userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during remove bank account. Please try again later.";
            }
            return result;
        }
    }
}
