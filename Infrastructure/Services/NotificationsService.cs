using Application.Dto.Notification;
using Application.Interfaces;
using Expo.Server.Client;
using Expo.Server.Models;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class PushNotificationsService : IPushNotificationService
    {
        private PushApiClient _Client { get; set; }
        public PushNotificationsService()
        {
            _Client = new PushApiClient();
        }

        public async Task<bool> SendNotification(List<string> expoPushNotificationTokens, string title, string body, NotificationDto notificationDto)
        {
            bool success = false;

            var pushTicketReq = new PushTicketRequest()
            {
                PushTo = expoPushNotificationTokens,
                PushBadgeCount = 7,
                PushTitle = title,
                PushBody = body,
                PushPriority = "high",
                PushChannelId = "default",
                PushData = JsonSerializer.Serialize(notificationDto)
            };
            try
            {
                var result = await _Client.PushSendAsync(pushTicketReq);
                success = result.PushTicketErrors != null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
            return success;
        }
    }
}
