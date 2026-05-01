using System.ComponentModel.DataAnnotations;

namespace Domain.SaltEdge
{
    public class SaltEdgeCustomer
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser? User { get; set; }
        public required string CustomerId { get; set; }
        public required string Identifier { get; set; }
        public List<SaltEdgeConnection> Connections { get; set; } = new();
    }
}
