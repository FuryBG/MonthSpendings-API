using Application.Dto.Budget;
using Domain;

namespace Application.Mappers
{
    public static class MonthlyBudgetMapper
    {
        public static MonthlyBudgetDto ToDto(this MonthlyBudget monthlyBudget)
        {
            return new MonthlyBudgetDto()
            {
                Id = monthlyBudget.Id,
                StartDate = monthlyBudget.StartDate,
                EndDate = monthlyBudget.EndDate,
                Categories = monthlyBudget.BudgetCategories != null
                                    ? monthlyBudget.BudgetCategories.Select(category => category.ToDto()).ToList()
                                    : [],

            };
        }
        public static MonthlyBudget ToEntity(this MonthlyBudgetDto dto)
        {
            return new MonthlyBudget()
            {
                Id = dto.Id,
                StartDate = dto.StartDate ?? DateTime.UtcNow,
                EndDate = dto.EndDate ?? DateTime.UtcNow.AddMonths(1),
                BudgetCategories = dto.Categories != null
                                    ? dto.Categories.Select(category => category.ToEntity()).ToList()
                                    : [],

            };
        }
    }
}
