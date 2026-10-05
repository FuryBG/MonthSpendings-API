using System.Text.Json.Serialization;

namespace Application.Dto.Statistics
{
    public class RangeSummaryDto
    {
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
        [JsonPropertyName("averagePerPeriod")]
        public decimal AveragePerPeriod { get; set; }
        [JsonPropertyName("periods")]
        public List<RangePeriodDto> Periods { get; set; } = new();
        [JsonPropertyName("categories")]
        public List<RangeCategoryDto> Categories { get; set; } = new();
        [JsonPropertyName("topSpendings")]
        public List<RangeSpendingDto> TopSpendings { get; set; } = new();
    }
}
