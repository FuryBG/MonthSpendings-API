using System.ComponentModel.DataAnnotations;

namespace Domain.Bank
{
    public class TransactionCategoryRule
    {
        [Key]
        public int Id { get; set; }
        public required int UserId { get; set; }
        public Domain.AppUser? User { get; set; }
        public required string Keyword { get; set; }
        public required int CategoryId { get; set; }
        public Domain.BudgetCategory? Category { get; set; }
    }
}
