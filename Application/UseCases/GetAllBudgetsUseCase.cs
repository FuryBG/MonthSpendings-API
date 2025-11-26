using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

namespace Application.UseCases
{
    public interface IGetAllBudgetsUseCase
    {
        Task<CaseResult<List<BudgetDto>>> InvokeAsync();
    }

    public class GetAllBudgetsUseCase : IGetAllBudgetsUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public GetAllBudgetsUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<List<BudgetDto>>> InvokeAsync()
        {
            var result = new CaseResult<List<BudgetDto>>([]);
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                List<Budget> budgets = await _UnitOfWork.BudgetRepository.GetUserBudgets(userId);
                List<BudgetDto> budgetsDto = budgets.Select(budget => budget.ToDto()).ToList();
                result.Data = budgetsDto;
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
