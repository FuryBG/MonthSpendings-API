using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Dto
{
    /// <summary>Raw wallet notification posted by the phone; parsing happens on the server.</summary>
    public class CreateNotificationTransactionDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [Required]
        [JsonPropertyName("text")]
        public string Text { get; set; } = null!;
        [JsonPropertyName("packageName")]
        public string? PackageName { get; set; }
        [JsonPropertyName("postedAt")]
        public DateTime PostedAt { get; set; }
    }
}
