using System.ComponentModel.DataAnnotations;

namespace Domain.Bank
{
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }
        public required string Iban { get; set; }
        public required string HolderName { get; set; }
        public required string Currency { get; set; }
        public required int ConsentId { get; set; }
        public BankConsent? Consent { get; set; }
    }
}
