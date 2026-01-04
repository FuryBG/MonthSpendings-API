using System.Text.Json.Serialization;

namespace Application.Dto.Budget
{
    public class CurrencyDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
    }
}
