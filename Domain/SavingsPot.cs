using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class SavingsPot
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        [ForeignKey(nameof(Currency))]
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; } = null!;
        [ForeignKey(nameof(CreatedBy))]
        public int CreatedByUserId { get; set; }
        public AppUser CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<AppUser> Users { get; set; } = new();
        public List<SavingsContribution> Contributions { get; set; } = new();
        public List<SavingsPotInvite> Invites { get; set; } = new();
    }
}
