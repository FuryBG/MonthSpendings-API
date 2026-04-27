using System.Text.Json.Serialization;

namespace Application.Dto.Savings
{
    public class SavingsPotInviteDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("savingsPotId")]
        public int SavingsPotId { get; set; }
        [JsonPropertyName("receiverEmail")]
        public required string ReceiverEmail { get; set; }
        [JsonPropertyName("validTo")]
        public DateTime? ValidTo { get; set; }
        [JsonPropertyName("accepted")]
        public bool? Accepted { get; set; }
    }
}
