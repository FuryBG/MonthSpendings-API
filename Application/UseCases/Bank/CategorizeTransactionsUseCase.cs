using Application.Contracts;
using Application.Dto.Bank;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface ICategorizeTransactionsUseCase
    {
        Task<CaseResult<SpendingDto?>> InvokeAsync(BankTransactionDto dto, CancellationToken cancellationToken);
    }

    public class CategorizeTransactionsUseCase : ICategorizeTransactionsUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<CategorizeTransactionsUseCase> _Logger;

        public CategorizeTransactionsUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CategorizeTransactionsUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<SpendingDto?>> InvokeAsync(BankTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = new CaseResult<SpendingDto?>();
            result.Successful = true;

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(dto.CategoryId, userId);

                if (category == null)
                {
                    _Logger.LogWarning("Category {CategoryId} not found for user {UserId} when categorizing transaction", dto.CategoryId, userId);
                    result.Successful = false;
                    result.ErrorMessage = "Something got wrong during categorize the transaction. Please try again later.";
                    return result;
                }

                AppUser currentUser = category.Budget.Users.First(u => u.Id == userId);

                BudgetPeriod? budgetPeriod = category.Budget.BudgetPeriods.FirstOrDefault();

                if (budgetPeriod == null)
                {
                    _Logger.LogWarning("Active budget period not found for category {CategoryId}", dto.CategoryId);
                    result.Successful = false;
                    result.ErrorMessage = "Something got wrong during categorize the transaction. Please try again later.";
                    return result;
                }

                await _UnitOfWork.BeginTransactionAsync();

                Spending spending = _UnitOfWork.CategorySpendingsRepository.AddSpending(new Spending()
                {
                    Amount = dto.Amount,
                    BankTransactionId = dto.Id,
                    Date = DateTime.SpecifyKind(dto.BookingDate, DateTimeKind.Utc),
                    BudgetCategoryId = category.Id,
                    BudgetPeriodId = budgetPeriod.Id,
                    CreatedByUserId = userId,
                });

                await _UnitOfWork.CommitAsync();
                await _UnitOfWork.BankTransactionRepository.CategorizeAsync([dto.Id], spending.Id, cancellationToken);
                await _UnitOfWork.CommitTransactionAsync();
                result.Data = spending.ToDto();
                result.Data.BankTransaction = dto;
                result.Data.CreatedByEmail = currentUser.Email;
                result.Data.CreatedByName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                _Logger.LogInformation("Transaction {TransactionId} categorized into category {CategoryId} by user {UserId}", dto.Id, dto.CategoryId, userId);
            }
            catch (Exception ex)
            {
                await _UnitOfWork.RollbackTransactionAsync();
                _Logger.LogError(ex, "Error categorizing transaction for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting banks. Please try again later.";
            }
            return result;
        }
    }
}
