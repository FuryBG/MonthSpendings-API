using Application.Dto.Budget;
using Domain;

namespace Application.Mappers
{
    public static class BudgetCategoryMapper
    {
        public static BudgetCategoryDto ToDto(this BudgetCategory budgetCategory)
        {
            return new BudgetCategoryDto()
            {
                Id = budgetCategory.Id,
                Name = budgetCategory.Name,
                BudgetId = budgetCategory.BudgetId,
                Spendings = budgetCategory.Spendings != null
                            ? budgetCategory.Spendings.Select(spending => spending.ToDto()).ToList()
                            : [],
            };
        }
        public static BudgetCategory ToEntity(this BudgetCategoryDto dto)
        {
            return new BudgetCategory()
            {
                Id = dto.Id,
                Name = dto.Name,
                BudgetId = dto.BudgetId,
                Spendings = dto.Spendings != null
                            ? dto.Spendings.Select(spending => spending.ToEntity()).ToList()
                            : [],
            };
        }
    }
}
