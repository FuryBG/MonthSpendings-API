using System.Text.Json.Serialization;

namespace Application.Dto
{
    public class GoogleUserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("givenName")]
        public string? FirstName { get; set; }
        [JsonPropertyName("familyName")]
        public string? LastName { get; set; }
        [JsonPropertyName("photo")]
        public string? PhotoAddress { get; set; }
        [JsonPropertyName("notificationToken")]
        public string? NotificationToken { get; set; }
    }
}
