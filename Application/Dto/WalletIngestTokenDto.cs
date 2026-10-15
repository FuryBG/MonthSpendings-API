using System.Text.Json.Serialization;

namespace Application.Dto
{
    public record WalletIngestTokenDto([property: JsonPropertyName("token")] string Token);
}
