using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;

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
        public DeleteBudgetUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = $"Something got wrong during deleting budget with id {budgetId}. Please try again later.";
            }

            return result;
        }
    }
}
