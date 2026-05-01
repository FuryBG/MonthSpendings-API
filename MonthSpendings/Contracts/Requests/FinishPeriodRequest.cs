using Application.Dto.Budget;
using System.Text.Json.Serialization;

namespace MonthSpendings.Contracts.Requests
{
    public class FinishPeriodRequest
    {
        [JsonPropertyName("budget")]
        public BudgetDto Budget { get; set; } = null!;
        [JsonPropertyName("savingsPotId")]
        public int? SavingsPotId { get; set; }
    }
}
