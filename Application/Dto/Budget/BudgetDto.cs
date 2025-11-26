using Domain;
using System.Text.Json.Serialization;

namespace Application.Dto.Budget
{
    public class BudgetDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("budgetPeriods")]
        public List<BudgetPeriodDto> BudgetPeriods { get; set; }
        [JsonPropertyName("budgetCategories")]
        public List<BudgetCategoryDto> BudgetCategories { get; set; }
        [JsonPropertyName("users")]
        public List<AppUserDto>? Users { get; set; }
    }

}
