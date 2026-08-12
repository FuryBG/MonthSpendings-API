using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationTransactions
{
    public interface IGetUncategorizedNotificationTransactionsUseCase
    {
        Task<CaseResult<List<NotificationTransactionDto>>> InvokeAsync(CancellationToken cancellationToken);
    }

    public class GetUncategorizedNotificationTransactionsUseCase : IGetUncategorizedNotificationTransactionsUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<GetUncategorizedNotificationTransactionsUseCase> _Logger;

        public GetUncategorizedNotificationTransactionsUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetUncategorizedNotificationTransactionsUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<NotificationTransactionDto>>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<NotificationTransactionDto>>();
            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                List<NotificationTransaction> transactions = await _UnitOfWork.NotificationTransactionRepository.GetUncategorizedByUserAsync(userId, cancellationToken);
                result.Successful = true;
                result.Data = transactions.Select(t => new NotificationTransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    MerchantName = t.MerchantName,
                    ReceivedAt = t.ReceivedAt,
                    Categorized = t.Categorized,
                }).ToList();
                _Logger.LogInformation("Retrieved {Count} uncategorized notification transactions for user {UserId}", result.Data.Count, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving uncategorized notification transactions for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Failed to retrieve pending transactions. Please try again.";
            }
            return result;
        }
    }
}
