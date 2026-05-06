using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class BudgetCategory
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        [ForeignKey(nameof(Budget))]
        public int BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public required List<Spending> Spendings { get; set; }
        public bool IsDeleted { get; set; }
    }
}
