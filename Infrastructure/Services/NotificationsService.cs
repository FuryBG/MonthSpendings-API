using Application.Dto.Notification;
using Application.Interfaces;
using Expo.Server.Client;
using Expo.Server.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class PushNotificationsService : IPushNotificationService
    {
        private PushApiClient _Client { get; set; }
        private readonly ILogger<PushNotificationsService> _Logger;
        public PushNotificationsService(ILogger<PushNotificationsService> logger)
        {
            _Client = new PushApiClient();
            _Logger = logger;
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
                success = result.PushTicketErrors == null;
                if (success) _Logger.LogInformation("Push notification sent to {TokenCount} device(s)", expoPushNotificationTokens.Count);
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Push notification send failed");

            }
            return success;
        }
    }
}
