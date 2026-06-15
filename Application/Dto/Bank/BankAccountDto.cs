namespace Application.Dto.Bank
{
    public class BankAccountDto
    {
        public int Id { get; set; }
        public string Iban { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
    }
}
