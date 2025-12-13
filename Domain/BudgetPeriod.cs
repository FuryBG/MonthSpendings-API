using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class BudgetPeriod
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Budget))]
        public int BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        // .HasDefaultValueSql("timezone('utc', now())");
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
