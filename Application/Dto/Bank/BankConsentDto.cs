namespace Application.Dto.Bank
{
    public class BankConsentDto
    {
        public int Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime ValidTo { get; set; }
        public List<BankAccountDto> BankAccounts { get; set; } = new List<BankAccountDto>();
    }
}
