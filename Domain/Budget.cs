using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AppUser> Users { get; set; }
        public List<BudgetPeriod> BudgetPeriods { get; set; }
        public List<BudgetCategory> BudgetCategories { get; set; }
    }
}
