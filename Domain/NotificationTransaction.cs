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
        [MaxLength(4)]
        public string? CardLast4 { get; set; }
        public string? RawTitle { get; set; }
        public string? RawBody { get; set; }
        public string? PackageName { get; set; }
        /// <summary>The amount couldn't be read from the notification. Kept with the raw text for debugging; never shown as pending.</summary>
        public bool ParseFailed { get; set; }
        public DateTime ReceivedAt { get; set; }
        public bool Categorized { get; set; }
        public bool IsDeleted { get; set; }
        public int? SpendingId { get; set; }
        public Spending? Spending { get; set; }
    }
}
