using System.Text.Json.Serialization;

namespace Application.Dto.Statistics
{
    public class RangeCategoryDto
    {
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("share")]
        public decimal Share { get; set; }
    }
}
