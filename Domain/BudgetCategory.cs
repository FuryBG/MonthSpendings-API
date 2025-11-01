using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class BudgetCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int MonthlyBudgetId { get; set; }
        public MonthlyBudget MonthlyBudget { get; set; }
    }
}
