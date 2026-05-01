using System.Text.Json.Serialization;

namespace SaltEdge.Models.Callbacks
{
    public class SaltEdgeCallbackRequest
    {
        [JsonPropertyName("data")]
        public SaltEdgeCallbackData? Data { get; set; }
    }

    public class SaltEdgeCallbackData
    {
        [JsonPropertyName("connection_id")]
        public string? ConnectionId { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("custom_fields")]
        public Dictionary<string, string>? CustomFields { get; set; }

        [JsonPropertyName("stage")]
        public string? Stage { get; set; }

        [JsonPropertyName("error_class")]
        public string? ErrorClass { get; set; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
