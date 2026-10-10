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
    public interface ICreateBudgetCategoryUseCase
    {
        Task<CaseResult<BudgetCategoryDto?>> InvokeAsync(BudgetCategoryDto budgetcategoryDto);
    }

    public class CreateBudgetCategoryUseCase : ICreateBudgetCategoryUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<CreateBudgetCategoryUseCase> _Logger;
        public CreateBudgetCategoryUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CreateBudgetCategoryUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }
        public async Task<CaseResult<BudgetCategoryDto?>> InvokeAsync(BudgetCategoryDto budgetcategoryDto)
        {
            var result = new CaseResult<BudgetCategoryDto?>();
            result.Successful = true;

            int userId = _UserService.GetUserId();
            Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetcategoryDto.BudgetId, userId);

            if (budget == null)
            {
                _Logger.LogInformation("Budget {BudgetId} not found when creating category for user {UserId}", budgetcategoryDto.BudgetId, userId);
                return CaseResult<BudgetCategoryDto?>.Error(Messages.BudgetNotFound);

            }
            BudgetCategory newCategory = budgetcategoryDto.ToEntity();
            newCategory.Spendings.ForEach(spending => spending.CreatedByUserId = userId);
            BudgetCategory addedCategory = _UnitOfWork.BudgetCategoryRepository.CreateCategory(newCategory);
            await _UnitOfWork.CommitAsync();
            result.Data = addedCategory.ToDto();
            _Logger.LogInformation("Category {CategoryId} created in budget {BudgetId} by user {UserId}", result.Data!.Id, budgetcategoryDto.BudgetId, userId);

            return result;
        }
    }
}
