using Application.Dto.Budget;
using System.Text.Json.Serialization;

namespace Application.Dto.Savings
{
    public class SavingsPotDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("currency")]
        public CurrencyDto Currency { get; set; } = null!;
        [JsonPropertyName("totalSaved")]
        public decimal TotalSaved { get; set; }
        [JsonPropertyName("createdByUserId")]
        public int CreatedByUserId { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("users")]
        public List<AppUserDto> Users { get; set; } = new();
        [JsonPropertyName("recentContributions")]
        public List<SavingsContributionDto> RecentContributions { get; set; } = new();
    }
}
