namespace Application.Dto.Bank
{
    public class BankConsentDto
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime ValidTo { get; set; }
        public string State { get; set; } = string.Empty;
        public List<BankAccountDto> BankAccounts { get; set; } = new List<BankAccountDto>();
    }
}
