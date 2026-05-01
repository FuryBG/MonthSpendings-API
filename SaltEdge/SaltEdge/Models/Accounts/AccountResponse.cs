using System.Text.Json.Serialization;

namespace SaltEdge.Models.Accounts
{
    public class AccountResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("connection_id")]
        public string? ConnectionId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("currency_code")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("extra")]
        public AccountExtra? Extra { get; set; }
    }

    public class AccountExtra
    {
        [JsonPropertyName("iban")]
        public string? Iban { get; set; }

        [JsonPropertyName("holder_name")]
        public string? HolderName { get; set; }
    }
}
