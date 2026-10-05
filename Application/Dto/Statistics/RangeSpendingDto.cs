using System.Text.Json.Serialization;

namespace Application.Dto.Statistics
{
    public class RangeSpendingDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
