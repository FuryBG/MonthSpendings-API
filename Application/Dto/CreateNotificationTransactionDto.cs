using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class CreateNotificationTransactionDto
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;
        [JsonPropertyName("merchantName")]
        public string MerchantName { get; set; } = null!;
        [JsonPropertyName("rawTitle")]
        public string? RawTitle { get; set; }
        [JsonPropertyName("rawBody")]
        public string? RawBody { get; set; }
    }
}
