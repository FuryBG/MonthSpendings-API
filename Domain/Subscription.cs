namespace Domain
{
    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        public string EventType { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
