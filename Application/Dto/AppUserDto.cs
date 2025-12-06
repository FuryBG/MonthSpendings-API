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
        public string NotificationToken { get; set; }
        [JsonPropertyName("googleId")]
        public string GoogleId { get; set; }
        [JsonPropertyName("googlePhotoAddress")]
        public string GooglePhotoAddress { get; set; }
    }
}
