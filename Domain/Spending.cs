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
        public int? NotificationTransactionId { get; set; }
        public NotificationTransaction? NotificationTransaction { get; set; }
        public int CreatedByUserId { get; set; }
        public AppUser CreatedBy { get; set; } = null!;
    }
}
