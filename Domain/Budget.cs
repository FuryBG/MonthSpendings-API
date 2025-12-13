using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<AppUser> Users { get; set; } = new();
        public List<BudgetPeriod> BudgetPeriods { get; set; } = new();
        public List<BudgetCategory> BudgetCategories { get; set; } = new();
    }
}
