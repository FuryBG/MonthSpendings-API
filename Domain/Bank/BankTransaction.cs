using Domain.Bank.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Bank
{
    public class BankTransaction
    {
        [Key]
        public int Id { get; set; }
        public required string TransactionId { get; set; }
        public int BankAccountId { get; set; }
        public BankAccount? BankAccount { get; set; }
        public required string Currency { get; set; }
        public decimal Amount { get; set; }
        public string? MerchantCode { get; set; }
        public string? CreditorName { get; set; }
        public string? Description { get; set; }
        public required TransactionStatus Status { get; set; }
        public DateTime BookingDate { get; set; }
        public bool Categorized { get; set; }
        public bool Determined { get; set; } = true;
        public int? SpendingId { get; set; }
        public Spending? Spending { get; set; }
    }
}
