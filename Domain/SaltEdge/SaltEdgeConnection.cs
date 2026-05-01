using Domain.SaltEdge.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.SaltEdge
{
    public class SaltEdgeConnection
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser? User { get; set; }
        public int SaltEdgeCustomerId { get; set; }
        public SaltEdgeCustomer? Customer { get; set; }
        public Guid LocalSessionId { get; set; }
        public string? ConnectionId { get; set; }
        public SaltEdgeConnectionStatus State { get; set; }
        public DateTime? ExpiresOn { get; set; }
        public required string BankImgUrl { get; set; }
        public required string ProviderName { get; set; }
        public required string ProviderCode { get; set; }
        public required string CountryCode { get; set; }
        public string? LastStage { get; set; }
        public string? LastErrorClass { get; set; }
        public string? LastErrorMessage { get; set; }
        public DateTime LastSync { get; set; }
        public DateTime? SyncStartedAt { get; set; }
        public List<SaltEdgeAccount> Accounts { get; set; } = new();
    }
}
