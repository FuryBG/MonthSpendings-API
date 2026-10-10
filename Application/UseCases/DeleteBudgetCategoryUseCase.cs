using Application.Resources;
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

            int userId = _UserService.GetUserId();
            BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(budgetCategoryId, userId);

            if (budgetCategory == null)
            {
                _Logger.LogInformation("Budget category {CategoryId} not found for user {UserId} on delete", budgetCategoryId, userId);
                return CaseResult<int?>.Error(Messages.CategoryNotFound);

            }

            BudgetCategory addedCategory = _UnitOfWork.BudgetCategoryRepository.DeleteCategory(budgetCategory);
            await _UnitOfWork.TransactionCategoryRuleRepository.DeleteByCategoryIdAsync(budgetCategoryId, new CancellationToken());
            await _UnitOfWork.CommitAsync();
            result.Data = budgetCategoryId;
            _Logger.LogInformation("Budget category {CategoryId} deleted by user {UserId}", budgetCategoryId, userId);

            return result;
        }
    }
}
