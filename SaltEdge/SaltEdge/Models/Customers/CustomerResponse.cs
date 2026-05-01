using System.Text.Json.Serialization;

namespace SaltEdge.Models.Customers
{
    public class CustomerResponse
    {
        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("identifier")]
        public string? Identifier { get; set; }
    }
}
