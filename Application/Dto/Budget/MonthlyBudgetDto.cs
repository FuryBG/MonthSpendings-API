using System.Text.Json.Serialization;

namespace Application.Dto.Budget
{
    public class MonthlyBudgetDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }
        [JsonPropertyName("budgetCategories")]
        public List<BudgetCategoryDto>? Categories { get; set; }

    }
}
