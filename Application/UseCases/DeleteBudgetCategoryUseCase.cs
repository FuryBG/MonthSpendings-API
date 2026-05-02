using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<DeleteBudgetCategoryUseCase> _Logger;
        public DeleteBudgetCategoryUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<DeleteBudgetCategoryUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
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
                    _Logger.LogWarning("Budget category {CategoryId} not found for deletion", budgetCategoryId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget to delete.";
                    return result;

                }

                BudgetCategory addedCategory = _UnitOfWork.BudgetCategoryRepository.DeleteCategory(budgetCategory);
                await _UnitOfWork.CommitAsync();
                result.Data = budgetCategoryId;
                _Logger.LogInformation("Budget category {CategoryId} deleted by user {UserId}", budgetCategoryId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error deleting budget category {CategoryId}", budgetCategoryId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during deleting category. Please try again later.";
            }

            return result;
        }
    }
}
