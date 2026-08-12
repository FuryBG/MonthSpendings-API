using Application.Contracts;
using Application.Dto;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationTransactions
{
    public interface ICategorizeNotificationTransactionUseCase
    {
        Task<CaseResult<SpendingDto?>> InvokeAsync(CategorizeNotificationTransactionDto dto, CancellationToken cancellationToken);
    }

    public class CategorizeNotificationTransactionUseCase : ICategorizeNotificationTransactionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<CategorizeNotificationTransactionUseCase> _Logger;

        public CategorizeNotificationTransactionUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CategorizeNotificationTransactionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<SpendingDto?>> InvokeAsync(CategorizeNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = new CaseResult<SpendingDto?>();
            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();

                NotificationTransaction? transaction = await _UnitOfWork.NotificationTransactionRepository.GetByIdAsync(dto.Id, userId, cancellationToken);
                if (transaction == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Transaction not found.";
                    return result;
                }

                BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(dto.CategoryId, userId);
                if (category == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Category not found.";
                    return result;
                }

                BudgetPeriod? budgetPeriod = category.Budget.BudgetPeriods.FirstOrDefault();
                if (budgetPeriod == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "No active budget period found.";
                    return result;
                }

                AppUser currentUser = category.Budget.Users.First(u => u.Id == userId);

                await _UnitOfWork.BeginTransactionAsync();

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
                await _UnitOfWork.CommitTransactionAsync();

                if (dto.CreateRule)
                {
                    TransactionCategoryRule rule = new TransactionCategoryRule
                    {
                        UserId = userId,
                        Keyword = transaction.MerchantName.Trim(),
                        CategoryId = dto.CategoryId,
                    };
                    await _UnitOfWork.TransactionCategoryRuleRepository.AddAsync(rule, cancellationToken);
                    await _UnitOfWork.CommitAsync();
                    _Logger.LogInformation("Created auto-categorization rule for merchant '{Merchant}' → category {CategoryId}", transaction.MerchantName, dto.CategoryId);
                }

                result.Successful = true;
                result.Data = spending.ToDto();
                result.Data.CreatedByEmail = currentUser.Email;
                result.Data.CreatedByName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                _Logger.LogInformation("Notification transaction {Id} categorized into category {CategoryId} by user {UserId}", dto.Id, dto.CategoryId, userId);
            }
            catch (Exception ex)
            {
                await _UnitOfWork.RollbackTransactionAsync();
                _Logger.LogError(ex, "Error categorizing notification transaction {Id} for user {UserId}", dto.Id, userId);
                result.Successful = false;
                result.ErrorMessage = "Failed to categorize the transaction. Please try again.";
            }
            return result;
        }
    }
}
