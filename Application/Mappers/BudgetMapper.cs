using Application.Dto.Budget;
using Domain;

namespace Application.Mappers
{
    public static class BudgetMapper
    {
        public static BudgetDto ToDto(this Budget budget)
        {
            return new BudgetDto()
            {
                Id = budget.Id,
                Name = budget.Name,
                BudgetPeriods = budget.BudgetPeriods.Select(budgetPeriod => budgetPeriod.ToDto()).ToList(),
                BudgetCategories = budget.BudgetCategories != null
                                    ? budget.BudgetCategories.Select(category => category.ToDto()).ToList()
                                    : [],
                Users = budget.Users != null
                        ? budget.Users.Select(user => user.ToDto()).ToList()
                        : []
            };
        }

        public static Budget ToEntity(this BudgetDto dto)
        {
            return new Budget()
            {
                Id = dto.Id,
                Name = dto.Name,
                BudgetPeriods = dto.BudgetPeriods != null ? dto.BudgetPeriods.Select(budgetPeriodDto => budgetPeriodDto.ToEntity()).ToList() : [],
                BudgetCategories = dto.BudgetCategories != null
                                    ? dto.BudgetCategories.Select(category => category.ToEntity()).ToList()
                                    : [],
                Users = dto.Users != null
                        ? dto.Users.Select(user => user.ToEntity()).ToList()
                        : []
            };
        }
    }
}
