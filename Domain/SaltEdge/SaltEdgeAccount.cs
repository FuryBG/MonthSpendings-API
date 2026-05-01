using System.ComponentModel.DataAnnotations;

namespace Domain.SaltEdge
{
    public class SaltEdgeAccount
    {
        [Key]
        public int Id { get; set; }
        public required string AccountId { get; set; }
        public required string Iban { get; set; }
        public required string HolderName { get; set; }
        public required string Currency { get; set; }
        public required int ConnectionDbId { get; set; }
        public SaltEdgeConnection? Connection { get; set; }
        public List<SaltEdgeTransaction> Transactions { get; set; } = new();
    }
}
