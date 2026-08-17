using System.Text.Json.Serialization;

namespace Application.Dto.Statistics
{
    public class PeriodHistoryItemDto
    {
        [JsonPropertyName("periodId")]
        public int PeriodId { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        [JsonPropertyName("totalSpent")]
        public decimal TotalSpent { get; set; }
    }
}
