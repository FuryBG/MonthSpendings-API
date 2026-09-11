using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface ILeaveBudgetUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(int budgetId);
    }

    public class LeaveBudgetUseCase : ILeaveBudgetUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<LeaveBudgetUseCase> _Logger;

        public LeaveBudgetUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<LeaveBudgetUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(int budgetId)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetId, userId);

                if (budget == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Budget not found.";
                    return result;
                }

                if (budget.OwnerId == userId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "The owner cannot leave the budget. Delete it instead.";
                    return result;
                }

                var self = budget.Users.FirstOrDefault(u => u.Id == userId);

                if (self == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "You are not a member of this budget.";
                    return result;
                }

                budget.Users.Remove(self);
                await _UnitOfWork.CommitAsync();

                result.Data = true;
                _Logger.LogInformation("User {UserId} left budget {BudgetId}", userId, budgetId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error leaving budget {BudgetId}", budgetId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong. Please try again later.";
            }

            return result;
        }
    }
}
