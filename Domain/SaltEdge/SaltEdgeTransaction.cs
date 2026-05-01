using Domain;
using Domain.SaltEdge.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.SaltEdge
{
    public class SaltEdgeTransaction
    {
        [Key]
        public int Id { get; set; }
        public required string TransactionId { get; set; }
        public required string ExternalTransactionId { get; set; }
        public int SaltEdgeAccountId { get; set; }
        public SaltEdgeAccount? SaltEdgeAccount { get; set; }
        public required string Currency { get; set; }
        public decimal Amount { get; set; }
        public string? MerchantCode { get; set; }
        public string? Description { get; set; }
        public required SaltEdgeTransactionStatus Status { get; set; }
        public DateTime BookingDate { get; set; }
        public bool Categorized { get; set; }
        public int? SpendingId { get; set; }
        public Spending? Spending { get; set; }
    }
}
