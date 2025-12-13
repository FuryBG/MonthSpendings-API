using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class BudgetInvite
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Sender))]
        public int SenderId { get; set; }
        public AppUser Sender { get; set; } = null!;
        [ForeignKey(nameof(Receiver))]
        public int ReceiverId { get; set; }
        public AppUser Receiver { get; set; } = null!;

        [ForeignKey(nameof(Budget))]
        public int BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        // .HasDefaultValueSql("timezone('utc', now()) + INTERVAL '2 days'");
        public DateTime ValidTo { get; set; }
        public bool? Accepted { get; set; }
    }
}
