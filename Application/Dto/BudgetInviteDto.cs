using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class BudgetInviteDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("receiverEmail")]
        public required string ReceiverEmail { get; set; }
        [JsonPropertyName("budgetId")]
        public int BudgetId { get; set; }
        [JsonPropertyName("validTo")]
        public DateTime? ValidTo { get; set; }
        [JsonPropertyName("accepted")]
        public bool? Accepted { get; set; }
    }
}
