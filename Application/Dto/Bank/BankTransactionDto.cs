using System.Text.Json.Serialization;

namespace Application.Dto.Bank
{
    public class BankTransactionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonPropertyName("bankAccountId")]
        public int BankAccountId { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("merchantCode")]
        public string? MerchantCode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("bookingDate")]
        public DateTime BookingDate { get; set; }

        [JsonPropertyName("categorized")]
        public bool Categorized { get; set; }
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
    }
}