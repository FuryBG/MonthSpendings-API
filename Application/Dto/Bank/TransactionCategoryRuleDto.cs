using System.Text.Json.Serialization;

namespace Application.Dto.Bank
{
    public class TransactionCategoryRuleDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = string.Empty;

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
    }

    public class CreateTransactionCategoryRuleDto
    {
        [JsonPropertyName("keyword")]
        public required string Keyword { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }
    }
}
