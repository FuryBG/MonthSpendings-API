using System.Text.Json.Serialization;

namespace Application.Dto.Budget
{
    public class SpendingDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("amount")]
        public double Amount { get; set; }
        [JsonPropertyName("budgetCategoryId")]
        public int BudgetCategoryId { get; set; }
        [JsonPropertyName("budgetPeriodId")]
        public int BudgetPeriodId { get; set; }
    }
}
