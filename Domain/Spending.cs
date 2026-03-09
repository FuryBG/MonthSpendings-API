using Domain.Bank;

namespace Domain
{
    public class Spending
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int BudgetCategoryId { get; set; }
        public BudgetCategory BudgetCategory { get; set; } = null!;
        public int BudgetPeriodId { get; set; }
        public BudgetPeriod BudgetPeriod { get; set; } = null!;
        public int? BankTransactionId { get; set; }
        public BankTransaction? BankTransaction { get; set; }
    }
}
