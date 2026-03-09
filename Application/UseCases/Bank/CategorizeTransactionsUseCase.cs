using Application.Contracts;
using Application.Dto.Bank;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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

        public CategorizeTransactionsUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<SpendingDto?>> InvokeAsync(BankTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = new CaseResult<SpendingDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(dto.CategoryId, userId);

                if (category == null)
                {
                    Console.WriteLine($"Can't find category with id {dto.CategoryId} for user with Id: {userId}");
                    result.Successful = false;
                    result.ErrorMessage = "Something got wrong during categorize the transaction. Please try again later.";
                    return result;
                }

                BudgetPeriod? budgetPeriod = category.Budget.BudgetPeriods.FirstOrDefault();

                if (budgetPeriod == null)
                {
                    Console.WriteLine($"Can't find budget period for budget with id {category.Budget.Id} for user with Id: {userId}");
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
                });

                await _UnitOfWork.CommitAsync();
                await _UnitOfWork.BankTransactionRepository.CategorizeAsync([dto.Id], spending.Id, cancellationToken);
                await _UnitOfWork.CommitTransactionAsync();
                result.Data = spending.ToDto();
                result.Data.BankTransaction = dto;
            }
            catch (Exception ex)
            {
                await _UnitOfWork.RollbackTransactionAsync();
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting banks. Please try again later.";
            }
            return result;
        }
    }
}