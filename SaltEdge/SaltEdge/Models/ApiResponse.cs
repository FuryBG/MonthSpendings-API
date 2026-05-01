using System.Net;
using System.Text.Json.Serialization;

namespace SaltEdge.Models
{
    public class ApiResponse<T>
    {
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("meta")]
        public ApiMeta? Meta { get; set; }

        [JsonPropertyName("error")]
        public ApiError? Error { get; set; }
    }

    public class ApiMeta
    {
        [JsonPropertyName("next_id")]
        public string? NextId { get; set; }

        [JsonPropertyName("next_page")]
        public string? NextPage { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("time")]
        public DateTime? Time { get; set; }
    }
}
