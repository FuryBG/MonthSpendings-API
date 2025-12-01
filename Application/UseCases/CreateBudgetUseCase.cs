using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        public CreateBudgetUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
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
                    result.Successful = false;
                    result.ErrorMessage = "Can't find your personal information. Please first login, to create a budget.";
                    //todo log
                    return result;
                }

                Budget budget = budgetDto.ToEntity();
                budget.Users.Add(existingUser);
                BudgetPeriod newBudgetPeriod = new BudgetPeriod() { StartDate = DateTime.UtcNow };
                budget.BudgetPeriods.Add(newBudgetPeriod);

                budget.BudgetCategories.ForEach(budgetCategory =>
                {
                    budgetCategory.Spendings.ForEach(spending => spending.BudgetPeriod = newBudgetPeriod);
                });

                Budget newBudget = _UnitOfWork.BudgetRepository.CreateBudget(budget);
                await _UnitOfWork.CommitAsync();

                result.Data = newBudget.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during budget creation. Please try again later.";
            }
            return result;
        }
    }
}
