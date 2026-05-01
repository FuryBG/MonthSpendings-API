using System.Text.Json.Serialization;

namespace SaltEdge.Models.Transactions
{
    public class TransactionResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("account_id")]
        public string? AccountId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("made_on")]
        public DateOnly? MadeOn { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency_code")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("extra")]
        public TransactionExtra? Extra { get; set; }
    }

    public class TransactionExtra
    {
        [JsonPropertyName("transaction_code")]
        public string? TransactionCode { get; set; }

        [JsonPropertyName("merchant_category_code")]
        public string? MerchantCategoryCode { get; set; }
    }
}
