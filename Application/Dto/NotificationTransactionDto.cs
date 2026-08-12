using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class NotificationTransactionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;
        [JsonPropertyName("merchantName")]
        public string MerchantName { get; set; } = null!;
        [JsonPropertyName("receivedAt")]
        public DateTime ReceivedAt { get; set; }
        [JsonPropertyName("categorized")]
        public bool Categorized { get; set; }
    }
}
