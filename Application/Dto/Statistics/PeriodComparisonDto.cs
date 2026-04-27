using System.Text.Json.Serialization;

namespace Application.Dto.Statistics
{
    public class PeriodComparisonDto
    {
        [JsonPropertyName("currentPeriod")]
        public PeriodSummaryDto CurrentPeriod { get; set; } = null!;
        [JsonPropertyName("previousPeriod")]
        public PeriodSummaryDto? PreviousPeriod { get; set; }
        [JsonPropertyName("totalDelta")]
        public decimal TotalDelta { get; set; }
        [JsonPropertyName("totalDeltaPercent")]
        public decimal? TotalDeltaPercent { get; set; }
    }

    public class PeriodSummaryDto
    {
        [JsonPropertyName("periodId")]
        public int PeriodId { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        [JsonPropertyName("totalSpent")]
        public decimal TotalSpent { get; set; }
        [JsonPropertyName("categories")]
        public List<CategoryComparisonDto> Categories { get; set; } = new();
    }

    public class CategoryComparisonDto
    {
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = "";
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
