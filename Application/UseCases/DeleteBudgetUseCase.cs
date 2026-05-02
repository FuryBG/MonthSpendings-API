using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IDeleteBudgetUseCase
    {
        Task<CaseResult<int?>> InvokeAsync(int spendingId);
    }

    public class DeleteBudgetUseCase : IDeleteBudgetUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<DeleteBudgetUseCase> _Logger;
        public DeleteBudgetUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<DeleteBudgetUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<int?>> InvokeAsync(int budgetId)
        {
            var result = new CaseResult<int?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetId, userId);

                if (budget == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = $"Can't find budget with id {budgetId} to delete. Please try again later.";
                    return result;
                }

                _UnitOfWork.BudgetRepository.DeleteBudget(budget);
                await _UnitOfWork.CommitAsync();
                result.Data = budgetId;
                _Logger.LogInformation("Budget {BudgetId} deleted by user {UserId}", budgetId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error deleting budget {BudgetId}", budgetId);
                result.Successful = false;
                result.ErrorMessage = $"Something got wrong during deleting budget with id {budgetId}. Please try again later.";
            }

            return result;
        }
    }
}
