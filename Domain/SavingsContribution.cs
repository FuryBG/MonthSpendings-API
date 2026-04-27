using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class SavingsContribution
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(SavingsPot))]
        public int SavingsPotId { get; set; }
        public SavingsPot SavingsPot { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        [ForeignKey(nameof(AddedBy))]
        public int AddedByUserId { get; set; }
        public AppUser AddedBy { get; set; } = null!;
    }
}
