using System.Text.Json.Serialization;

namespace SaltEdge.Models.Connections
{
    public class ConnectConnectionRequest
    {
        [JsonPropertyName("data")]
        public required ConnectConnectionRequestData Data { get; set; }
    }

    public class ConnectConnectionRequestData
    {
        [JsonPropertyName("customer_id")]
        public required string CustomerId { get; set; }

        [JsonPropertyName("consent")]
        public required ConsentRequest Consent { get; set; }

        [JsonPropertyName("attempt")]
        public required AttemptRequest Attempt { get; set; }

        [JsonPropertyName("provider")]
        public ProviderRequest? Provider { get; set; }

        [JsonPropertyName("return_connection_id")]
        public bool ReturnConnectionId { get; set; } = true;

        [JsonPropertyName("return_error_class")]
        public bool ReturnErrorClass { get; set; } = true;

        [JsonPropertyName("automatic_refresh")]
        public bool AutomaticRefresh { get; set; } = true;

        [JsonPropertyName("show_consent_confirmation")]
        public bool ShowConsentConfirmation { get; set; } = true;

        [JsonPropertyName("show_connection_details")]
        public bool ShowConnectionDetails { get; set; } = false;
    }

    public class ConsentRequest
    {
        [JsonPropertyName("scopes")]
        public required List<string> Scopes { get; set; }

        [JsonPropertyName("from_date")]
        public DateOnly? FromDate { get; set; }

        [JsonPropertyName("period_days")]
        public int? PeriodDays { get; set; }
    }

    public class AttemptRequest
    {
        [JsonPropertyName("return_to")]
        public required string ReturnTo { get; set; }

        [JsonPropertyName("fetch_scopes")]
        public required List<string> FetchScopes { get; set; }

        [JsonPropertyName("custom_fields")]
        public Dictionary<string, string> CustomFields { get; set; } = new();

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = "en";

        [JsonPropertyName("unduplication_strategy")]
        public string UnduplicationStrategy { get; set; } = "delete_duplicated";
    }

    public class ProviderRequest
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
    }
}
