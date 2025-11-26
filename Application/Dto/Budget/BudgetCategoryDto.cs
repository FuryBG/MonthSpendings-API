using System.Text.Json.Serialization;

namespace Application.Dto.Budget
{
    public class BudgetCategoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("budgetId")]
        public int BudgetId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("spendings")]
        public List<SpendingDto>? Spendings { get; set; }
    }
}
