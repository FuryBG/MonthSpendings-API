using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class SavingsPotInvite
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(SavingsPot))]
        public int SavingsPotId { get; set; }
        public SavingsPot SavingsPot { get; set; } = null!;
        [ForeignKey(nameof(Sender))]
        public int SenderId { get; set; }
        public AppUser Sender { get; set; } = null!;
        [ForeignKey(nameof(Receiver))]
        public int ReceiverId { get; set; }
        public AppUser Receiver { get; set; } = null!;
        public DateTime ValidTo { get; set; }
        public bool? Accepted { get; set; }
    }
}
