using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;
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
        private readonly ILogger<RemoveConnectedBankBySessionIdUseCase> _Logger;

        public RemoveConnectedBankBySessionIdUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<RemoveConnectedBankBySessionIdUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
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
                int deleted = await _UnitOfWork.BankConsentRepository.Delete(((BankConsent bankConsent) =>
                    bankConsent.SessionId == sessionId &&
                    bankConsent.UserId == userId), cancellationToken);

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
