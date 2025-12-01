namespace Domain
{
    public class Spending
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public int BudgetCategoryId { get; set; }
        public BudgetCategory BudgetCategory { get; set; }
        public int BudgetPeriodId { get; set; }
        public BudgetPeriod BudgetPeriod { get; set; }
    }
}
