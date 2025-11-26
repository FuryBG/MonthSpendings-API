using Application.Dto.Budget;
using Domain;

namespace Application.Mappers
{
    public static class SpendingMapper
    {
        public static SpendingDto ToDto(this Spending spending)
        {
            return new SpendingDto()
            {
                Id = spending.Id,
                Amount = spending.Amount,
                Date = spending.Date,
                Description = spending.Description,
                BudgetCategoryId = spending.BudgetCategoryId
            };
        }
        public static Spending ToEntity(this SpendingDto dto)
        {
            return new Spending()
            {
                Id = dto.Id,
                Amount = dto.Amount,
                Date = dto.Date ?? DateTime.UtcNow,
                Description = dto.Description,
                BudgetCategoryId = dto.BudgetCategoryId
            };
        }
    }
}
