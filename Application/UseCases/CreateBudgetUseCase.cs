using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface ICreateBudgetUseCase
    {
        Task<CaseResult<BudgetDto?>> InvokeAsync(BudgetDto budgetDto);
    }

    public class CreateBudgetUseCase : ICreateBudgetUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<CreateBudgetUseCase> _Logger;
        public CreateBudgetUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CreateBudgetUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }
        // THIS WILL CREATE THE WHOLE STRUCTURE Budget > MonthlyBudget> BudgetCategories > One Spending(init category budget)
        public async Task<CaseResult<BudgetDto?>> InvokeAsync(BudgetDto budgetDto)
        {
            var result = new CaseResult<BudgetDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                AppUser? existingUser = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (existingUser == null)
                {
                    _Logger.LogWarning("Unauthorized budget creation attempt — no user ID in context");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find your personal information. Please first login, to create a budget.";
                    return result;
                }

                var existingBudgets = await _UnitOfWork.BudgetRepository.GetUserBudgets(userId);

                if (!existingUser.IsPro && existingBudgets.Count >= 1)
                {
                    _Logger.LogWarning("Non-pro user {UserId} attempted to create a second budget", userId);
                    result.Successful = false;
                    result.ErrorMessage = "Free accounts are limited to one budget. Upgrade to Pro to create more.";
                    return result;
                }

                if (existingUser.IsPro && existingBudgets.Count >= 3)
                {
                    _Logger.LogWarning("Pro user {UserId} attempted to exceed the 3-budget limit", userId);
                    result.Successful = false;
                    result.ErrorMessage = "Pro accounts are limited to 3 budgets.";
                    return result;
                }

                Budget budget = budgetDto.ToEntity();
                budget.OwnerId = userId;
                budget.Users.Add(existingUser);
                BudgetPeriod newBudgetPeriod = new BudgetPeriod() { StartDate = DateTime.UtcNow };
                budget.BudgetPeriods.Add(newBudgetPeriod);

                budget.BudgetCategories.ForEach(budgetCategory =>
                {
                    budgetCategory.Spendings.ForEach(spending =>
                    {
                        spending.BudgetPeriod = newBudgetPeriod;
                        spending.CreatedByUserId = userId;
                    });
                });

                Budget newBudget = _UnitOfWork.BudgetRepository.CreateBudget(budget);
                await _UnitOfWork.CommitAsync();

                result.Data = newBudget.ToDto();
                _Logger.LogInformation("Budget {BudgetId} created for user {UserId}", result.Data!.Id, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error creating budget for user {UserId}", _UserService.GetUserId());
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during budget creation. Please try again later.";
            }
            return result;
        }
    }
}
