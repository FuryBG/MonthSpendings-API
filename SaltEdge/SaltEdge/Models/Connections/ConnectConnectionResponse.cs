using System.Text.Json.Serialization;

namespace SaltEdge.Models.Connections
{
    public class ConnectConnectionResponse
    {
        [JsonPropertyName("connect_url")]
        public string? ConnectUrl { get; set; }
    }
}
