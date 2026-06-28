using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class AccountDeleteRequest
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DeleteRequestStatus Status { get; set; } = DeleteRequestStatus.Pending;
    }

    public enum DeleteRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
