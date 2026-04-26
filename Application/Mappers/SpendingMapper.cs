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
                BudgetCategoryId = spending.BudgetCategoryId,
                BudgetPeriodId = spending.BudgetPeriodId,
                BankTransactionId = spending.BankTransactionId,
                BankTransaction = spending.BankTransaction != null ? spending.BankTransaction.ToDto() : null,
                CreatedByUserId = spending.CreatedByUserId,
                CreatedByEmail = spending.CreatedBy?.Email,
                CreatedByName = $"{spending.CreatedBy?.FirstName} {spending.CreatedBy?.LastName}".Trim(),
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
                BudgetCategoryId = dto.BudgetCategoryId,
                BudgetPeriodId = dto.BudgetPeriodId,
                BankTransactionId = dto.BankTransactionId,
            };
        }
    }
}
