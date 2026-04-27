using System.Text.Json.Serialization;

namespace Application.Dto.Savings
{
    public class SavingsHistoryDto
    {
        [JsonPropertyName("runningTotal")]
        public decimal RunningTotal { get; set; }
        [JsonPropertyName("months")]
        public List<MonthlyContributionDto> Months { get; set; } = new();
    }

    public class MonthlyContributionDto
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }
        [JsonPropertyName("month")]
        public int Month { get; set; }
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
        [JsonPropertyName("contributions")]
        public List<SavingsContributionDto> Contributions { get; set; } = new();
    }
}
