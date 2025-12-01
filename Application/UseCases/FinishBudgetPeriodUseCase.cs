using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

namespace Application.UseCases
{
    public interface IFinishBudgetPeriodUseCase
    {
        Task<CaseResult<BudgetDto?>> InvokeAsync(BudgetDto budgetDto);
    }

    public class FinishBudgetPeriodUseCase : IFinishBudgetPeriodUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public FinishBudgetPeriodUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }
        public async Task<CaseResult<BudgetDto?>> InvokeAsync(BudgetDto budgetDto)
        {
            var result = new CaseResult<BudgetDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                //TRACKED ENTITY, WILL UPDATE WHEN MODIFY ANY OF THE CHILDS
                Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetDto.Id, userId);

                if (budget == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = $"Can't find budget with id {budgetDto.Id} to delete. Please try again later.";
                    return result;
                }

                BudgetPeriod oldPeriod = budget.BudgetPeriods.First(budgetPeriod => budgetPeriod.EndDate == null);
                oldPeriod.EndDate = DateTime.UtcNow;

                BudgetPeriod newBudgetPeriod = new BudgetPeriod() { StartDate = DateTime.UtcNow };
                budget.BudgetPeriods.Add(newBudgetPeriod);

                foreach (var budgetCategory in budget.BudgetCategories)
                {
                    var dtoCategory = budgetDto.BudgetCategories
                        .FirstOrDefault(bc => bc.Id == budgetCategory.Id);

                    if (dtoCategory?.Spendings?.Any() == true)
                    {
                        Spending spending = dtoCategory.Spendings.First().ToEntity();
                        spending.Date = DateTime.UtcNow.AddSeconds(5);
                        spending.BudgetPeriod = newBudgetPeriod;
                        budgetCategory.Spendings!.Add(spending);
                    }
                }

                await _UnitOfWork.CommitAsync();
                result.Data = budget.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = $"Something got wrong during finish budget period on budget with id {budgetDto.Id}. Please try again later.";
            }

            return result;
        }
    }
}
