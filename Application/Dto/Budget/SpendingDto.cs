using Application.Dto.Bank;
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
        public string? Description { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("budgetCategoryId")]
        public int BudgetCategoryId { get; set; }
        [JsonPropertyName("budgetPeriodId")]
        public int BudgetPeriodId { get; set; }
        [JsonPropertyName("bankTransactionId")]
        public int? BankTransactionId { get; set; }
        [JsonPropertyName("bankTransaction")]
        public BankTransactionDto? BankTransaction { get; set; }
        [JsonPropertyName("createdByUserId")]
        public int CreatedByUserId { get; set; }
        [JsonPropertyName("createdByEmail")]
        public string? CreatedByEmail { get; set; }
        [JsonPropertyName("createdByName")]
        public string? CreatedByName { get; set; }
    }
}
