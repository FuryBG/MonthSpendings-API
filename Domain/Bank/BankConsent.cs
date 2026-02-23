using Domain.Bank.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Bank
{
    public class BankConsent
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public AppUser? User { get; set; }
        public required Guid SessionId { get; set; }
        public BankAccountStaatus State { get; set; }
        public required DateTime ExpiresOn { get; set; }
        public required string BankImgUrl { get; set; }
        public required string BankName { get; set; }
        public required string CountryCode { get; set; }
        public List<BankAccount> Accounts { get; set; } = [];
    }
}