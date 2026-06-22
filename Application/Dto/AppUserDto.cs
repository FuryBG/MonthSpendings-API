using Domain;
using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class AppUserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("email")]
        public required string Email { get; set; }
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
        [JsonPropertyName("notificationToken")]
        public required string NotificationToken { get; set; }
        [JsonPropertyName("googleId")]
        public required string GoogleId { get; set; }
        [JsonPropertyName("googlePhotoAddress")]
        public string? GooglePhotoAddress { get; set; }
        [JsonPropertyName("isPro")]
        public bool IsPro { get; set; }
        public List<BudgetInviteDto> SentBudgetInvites { get; set; } = new();
        public List<BudgetInviteDto> ReceivedBudgetInvites { get; set; } = new();
    }
}
