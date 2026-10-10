using Application.Dto.Notification;
using Domain;

namespace Application.Interfaces
{
    public interface IPushNotificationService
    {
        public Task<bool> SendLocalized(IEnumerable<AppUser> recipients, string titleKey, string bodyKey, NotificationDto notificationDto, params object[] bodyArgs);
    }
}
