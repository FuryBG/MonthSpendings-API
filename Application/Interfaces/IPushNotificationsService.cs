using Application.Dto.Notification;
using Application.Enums;

namespace Application.Interfaces
{
    public interface IPushNotificationsService
    {
        public Task<bool> SendNotification(List<string> expoPushNotificationTokens, string title, string body, NotificationDto notificationDto);
    }
}
