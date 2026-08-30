using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationTransactions
{
    public interface IDeleteNotificationTransactionUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(int id, CancellationToken cancellationToken);
    }

    public class DeleteNotificationTransactionUseCase : IDeleteNotificationTransactionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<DeleteNotificationTransactionUseCase> _Logger;

        public DeleteNotificationTransactionUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<DeleteNotificationTransactionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(int id, CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>();
            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();

                NotificationTransaction? transaction = await _UnitOfWork.NotificationTransactionRepository.GetByIdAsync(id, userId, cancellationToken);
                if (transaction == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Transaction not found.";
                    return result;
                }

                await _UnitOfWork.NotificationTransactionRepository.DeleteAsync(id, cancellationToken);

                result.Successful = true;
                result.Data = true;
                _Logger.LogInformation("Notification transaction {Id} soft-deleted by user {UserId}", id, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error deleting notification transaction {Id} for user {UserId}", id, userId);
                result.Successful = false;
                result.ErrorMessage = "Failed to delete the transaction. Please try again.";
            }
            return result;
        }
    }
}
