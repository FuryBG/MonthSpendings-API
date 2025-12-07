using Application.Interfaces;
using Expo.Server.Client;
using Expo.Server.Models;

namespace Infrastructure.Services
{
    public class PushNotificationsService : IPushNotificationsService
    {
        private PushApiClient _Client { get; set; }
        public PushNotificationsService()
        {
            _Client = new PushApiClient();
        }

        public async Task<bool> SendNotification(List<string> expoPushNotificationTokens, string title, string body)
        {
            bool success = false;

            var pushTicketReq = new PushTicketRequest()
            {
                PushTo = expoPushNotificationTokens,
                PushBadgeCount = 7,
                PushTitle = title,
                PushBody = body,
                PushPriority = "high",
                PushChannelId = "default"
            };
            try
            {
                var result = await _Client.PushSendAsync(pushTicketReq);
                success = result.PushTicketErrors != null;
            }
            catch (Exception e)
            {

            }
            return success;
        }
    }
}
