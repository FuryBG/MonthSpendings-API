using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required string Symbol { get; set; }
    }
}
