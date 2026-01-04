using Application.Dto.Notification;

namespace Application.Interfaces
{
    public interface IPushNotificationService
    {
        public Task<bool> SendNotification(List<string> expoPushNotificationTokens, string title, string body, NotificationDto notificationDto);
    }
}
