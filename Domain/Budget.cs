using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public int OwnerId { get; set; }
        public List<AppUser> Users { get; set; } = new();
        public List<BudgetPeriod> BudgetPeriods { get; set; } = new();
        public List<BudgetCategory> BudgetCategories { get; set; } = new();
    }
}
