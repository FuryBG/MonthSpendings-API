using System.Text.Json.Serialization;

namespace SaltEdge.Models.Customers
{
    public class CreateCustomerRequest
    {
        [JsonPropertyName("data")]
        public required CreateCustomerRequestData Data { get; set; }
    }

    public class CreateCustomerRequestData
    {
        [JsonPropertyName("identifier")]
        public required string Identifier { get; set; }
    }
}
