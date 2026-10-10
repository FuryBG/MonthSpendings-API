using Application.Dto.Notification;
using Application.Interfaces;
using Application.Resources;
using Domain;
using Expo.Server.Client;
using Expo.Server.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
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

        private async Task<bool> SendNotification(List<string> expoPushNotificationTokens, string title, string body, NotificationDto notificationDto)
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

                if (success)
                {
                    _Logger.LogInformation("Push notification sent to {TokenCount} device(s)", expoPushNotificationTokens.Count);
                }
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Push notification send failed");

            }
            return success;
        }

        public async Task<bool> SendLocalized(IEnumerable<AppUser> recipients, string titleKey, string bodyKey, NotificationDto notificationDto, params object[] bodyArgs)
        {
            var groups = recipients
                .Where(u => !string.IsNullOrEmpty(u.NotificationToken))
                .GroupBy(u => u.Language);

            bool success = true;
            foreach (var group in groups)
            {
                var culture = CultureInfo.GetCultureInfo(group.Key);
                string title = Messages.ResourceManager.GetString(titleKey, culture) ?? titleKey;
                string body = string.Format(culture, Messages.ResourceManager.GetString(bodyKey, culture) ?? bodyKey, bodyArgs);
                var tokens = group.Select(u => u.NotificationToken).Distinct().ToList();
                success &= await SendNotification(tokens, title, body, notificationDto);
            }
            return success;
        }
    }
}
