using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationTransactions
{
    public interface ICreateNotificationTransactionUseCase
    {
        Task<CaseResult<NotificationTransactionDto>> InvokeAsync(CreateNotificationTransactionDto dto, CancellationToken cancellationToken);
    }

    public class CreateNotificationTransactionUseCase : ICreateNotificationTransactionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<CreateNotificationTransactionUseCase> _Logger;

        public CreateNotificationTransactionUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CreateNotificationTransactionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<NotificationTransactionDto>> InvokeAsync(CreateNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = new CaseResult<NotificationTransactionDto>();
            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();

                var transaction = new NotificationTransaction
                {
                    UserId = userId,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    MerchantName = dto.MerchantName,
                    RawTitle = dto.RawTitle,
                    RawBody = dto.RawBody,
                    ReceivedAt = DateTime.UtcNow,
                    Categorized = false,
                };

                await _UnitOfWork.NotificationTransactionRepository.AddAsync(transaction, cancellationToken);
                await _UnitOfWork.CommitAsync();

                // Auto-apply matching rule if one exists
                TransactionCategoryRule? rule = await _UnitOfWork.TransactionCategoryRuleRepository.FindMatchingRuleAsync(userId, dto.MerchantName, cancellationToken);
                if (rule != null)
                {
                    BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(rule.CategoryId, userId);
                    BudgetPeriod? budgetPeriod = category?.Budget.BudgetPeriods.FirstOrDefault();
                    if (category != null && budgetPeriod != null)
                    {
                        Spending spending = _UnitOfWork.CategorySpendingsRepository.AddSpending(new Spending
                        {
                            Amount = -transaction.Amount,
                            NotificationTransactionId = transaction.Id,
                            Date = transaction.ReceivedAt,
                            BudgetCategoryId = category.Id,
                            BudgetPeriodId = budgetPeriod.Id,
                            CreatedByUserId = userId,
                        });
                        await _UnitOfWork.CommitAsync();
                        await _UnitOfWork.NotificationTransactionRepository.CategorizeAsync(transaction.Id, spending.Id, cancellationToken);
                        transaction.Categorized = true;
                        _Logger.LogInformation("Auto-categorized notification transaction {Id} via rule for merchant {Merchant}", transaction.Id, dto.MerchantName);
                    }
                }

                result.Successful = true;
                result.Data = new NotificationTransactionDto
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    MerchantName = transaction.MerchantName,
                    ReceivedAt = transaction.ReceivedAt,
                    Categorized = transaction.Categorized,
                };
                _Logger.LogInformation("Created notification transaction {Id} for user {UserId}, merchant {Merchant}", transaction.Id, userId, dto.MerchantName);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error creating notification transaction for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Failed to save the transaction. Please try again.";
            }
            return result;
        }
    }
}
