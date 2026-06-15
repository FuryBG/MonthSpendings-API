namespace Application.Dto.Bank
{
    public class BankOptionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Bic { get; set; } = string.Empty;
        public int MaximumConsentValidity { get; set; }
    }
}
