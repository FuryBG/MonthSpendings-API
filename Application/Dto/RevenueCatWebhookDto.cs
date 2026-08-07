using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class RevenueCatWebhookPayload
    {
        [JsonPropertyName("event")]
        public RevenueCatEvent Event { get; set; } = new();
    }

    public class RevenueCatEvent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("app_user_id")]
        public string AppUserId { get; set; } = string.Empty;

        [JsonPropertyName("original_app_user_id")]
        public string OriginalAppUserId { get; set; } = string.Empty;

        [JsonPropertyName("transferred_from")]
        public List<string> TransferredFrom { get; set; } = [];

        [JsonPropertyName("transferred_to")]
        public List<string> TransferredTo { get; set; } = [];

        [JsonPropertyName("expiration_at_ms")]
        public long? ExpirationAtMs { get; set; }

        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [JsonPropertyName("store")]
        public string? Store { get; set; }
    }
}
