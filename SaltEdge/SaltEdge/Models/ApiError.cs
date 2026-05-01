using System.Text.Json.Serialization;

namespace SaltEdge.Models
{
    public class ApiError
    {
        [JsonPropertyName("class")]
        public string? Class { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
