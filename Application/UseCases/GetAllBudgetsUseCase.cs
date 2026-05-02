using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<GetAllBudgetsUseCase> _Logger;
        public GetAllBudgetsUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetAllBudgetsUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
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
                _Logger.LogInformation("Retrieved {Count} budgets for user {UserId}", result.Data!.Count, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving budgets for user {UserId}", _UserService.GetUserId());
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting budgets. Please try again later.";
            }

            return result;
        }
    }
}
