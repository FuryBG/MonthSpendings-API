using Application.Resources;
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

            int userId = _UserService.GetUserId();
            var budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetId, userId);

            if (budget == null)
            {
                _Logger.LogInformation("Budget {BudgetId} not found for user {UserId} on leave", budgetId, userId);
                return CaseResult<bool>.Error(Messages.BudgetNotFound);
            }

            if (budget.OwnerId == userId)
            {
                _Logger.LogInformation("Owner {UserId} tried to leave their own budget {BudgetId}", userId, budgetId);
                return CaseResult<bool>.Error(Messages.BudgetOwnerCannotLeave);
            }

            var self = budget.Users.FirstOrDefault(u => u.Id == userId);

            if (self == null)
            {
                _Logger.LogWarning("User {UserId} can see budget {BudgetId} but is not in its member list", userId, budgetId);
                return CaseResult<bool>.Error(Messages.BudgetNotMember);
            }

            budget.Users.Remove(self);
            await _UnitOfWork.CommitAsync();

            result.Data = true;
            _Logger.LogInformation("User {UserId} left budget {BudgetId}", userId, budgetId);

            return result;
        }
    }
}
