using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class NotificationTransaction
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string MerchantName { get; set; } = null!;
        public string? RawTitle { get; set; }
        public string? RawBody { get; set; }
        public DateTime ReceivedAt { get; set; }
        public bool Categorized { get; set; }
        public bool IsDeleted { get; set; }
        public int? SpendingId { get; set; }
        public Spending? Spending { get; set; }
    }
}
