using Application.Enums;
using System.Text.Json.Serialization;

namespace Application.Dto.Notification
{
    public class NotificationDto
    {
        [JsonPropertyName("type")]
        public NotificationTypeEnum Type { get; set; }
    }
}
