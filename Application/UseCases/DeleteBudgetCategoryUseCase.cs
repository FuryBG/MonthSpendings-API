using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

namespace Application.UseCases
{
    public interface IDeleteBudgetCategoryUseCase
    {
        Task<CaseResult<int?>> InvokeAsync(int budgetCategoryId);
    }

    public class DeleteBudgetCategoryUseCase : IDeleteBudgetCategoryUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public DeleteBudgetCategoryUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }
        public async Task<CaseResult<int?>> InvokeAsync(int budgetCategoryId)
        {
            var result = new CaseResult<int?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(budgetCategoryId, userId);

                if (budgetCategory == null)
                {
                    Console.WriteLine($"Can't find budget category with id {budgetCategoryId} to delete.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget to delete.";
                    return result;

                }

                BudgetCategory addedCategory = _UnitOfWork.BudgetCategoryRepository.DeleteCategory(budgetCategory);
                await _UnitOfWork.CommitAsync();
                result.Data = budgetCategoryId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during deleting category. Please try again later.";
            }

            return result;
        }
    }
}
