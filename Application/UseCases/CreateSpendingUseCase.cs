using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

namespace Application.UseCases
{
    public interface ICreateSpendingUseCase
    {
        Task<CaseResult<SpendingDto?>> InvokeAsync(SpendingDto spendingDto);
    }

    public class CreateSpendingUseCase : ICreateSpendingUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public CreateSpendingUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<SpendingDto?>> InvokeAsync(SpendingDto spendingDto)
        {
            var result = new CaseResult<SpendingDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(spendingDto.BudgetCategoryId, userId);

                if (budgetCategory == null)
                {
                    Console.WriteLine($"Can't find category with id {spendingDto.Id} to add spending.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the category to add spending.";
                    return result;

                }

                Spending addedSpending = _UnitOfWork.CategorySpendingsRepository.AddSpending(spendingDto.ToEntity());
                await _UnitOfWork.CommitAsync();
                result.Data = addedSpending.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting budgets. Please try again later.";
            }

            return result;
        }
    }
}
