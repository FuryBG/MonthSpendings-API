using System.Text.Json.Serialization;

namespace SaltEdge.Models.Connections
{
    public class ConnectionResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("provider_name")]
        public string? ProviderName { get; set; }

        [JsonPropertyName("provider_code")]
        public string? ProviderCode { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("next_refresh_possible_at")]
        public DateTime? NextRefreshPossibleAt { get; set; }

        [JsonPropertyName("consent")]
        public ConsentResponse? Consent { get; set; }

        [JsonPropertyName("last_attempt")]
        public LastAttemptResponse? LastAttempt { get; set; }
    }

    public class ConsentResponse
    {
        [JsonPropertyName("expires_at")]
        public DateTime? ExpiresAt { get; set; }
    }

    public class LastAttemptResponse
    {
        [JsonPropertyName("custom_fields")]
        public Dictionary<string, string>? CustomFields { get; set; }
    }
}
