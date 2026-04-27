using System.Text.Json.Serialization;

namespace Application.Dto.Savings
{
    public class SavingsContributionDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("savingsPotId")]
        public int SavingsPotId { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        [JsonPropertyName("note")]
        public string? Note { get; set; }
        [JsonPropertyName("addedByUserId")]
        public int AddedByUserId { get; set; }
        [JsonPropertyName("addedByName")]
        public string? AddedByName { get; set; }
        [JsonPropertyName("addedByEmail")]
        public string? AddedByEmail { get; set; }
    }
}
