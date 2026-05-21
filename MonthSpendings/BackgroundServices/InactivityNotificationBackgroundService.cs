using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;

namespace MonthSpendings.BackgroundServices
{
    public class InactivityNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _ScopeFactory;
        private readonly ILogger<InactivityNotificationBackgroundService> _Logger;

        public InactivityNotificationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<InactivityNotificationBackgroundService> logger)
        {
            _ScopeFactory = scopeFactory;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("InactivityNotificationBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendInactivityNotificationsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error during inactivity notification check");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        private async Task CheckAndSendInactivityNotificationsAsync(CancellationToken stoppingToken)
        {
            await using var scope = _ScopeFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

            var users = await unitOfWork.UserRepository.GetAllUsersWithNotificationTokenAsync();
            var utcNow = DateTime.UtcNow;
            var notificationDto = new NotificationDto { Type = NotificationTypeEnum.InactivityReminder };

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Timezone))
                    continue;

                TimeZoneInfo tz;
                try
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                }
                catch (TimeZoneNotFoundException)
                {
                    _Logger.LogWarning("Unknown timezone '{Timezone}' for user {UserId}", user.Timezone, user.Id);
                    continue;
                }

                var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);

                if (localNow.Hour != 9 || localNow.Minute >= 5)
                    continue;

                if (user.LastVisited >= utcNow.AddHours(-24))
                    continue;

                var localToday = DateOnly.FromDateTime(localNow);
                if (user.LastInactivityNotificationSentAt.HasValue)
                {
                    var lastSentLocal = TimeZoneInfo.ConvertTimeFromUtc(user.LastInactivityNotificationSentAt.Value, tz);
                    if (DateOnly.FromDateTime(lastSentLocal) >= localToday)
                        continue;
                }

                var sent = await pushNotificationService.SendNotification(
                    [user.NotificationToken],
                    "We miss you!",
                    "Open Tavira to review your spending and stay on budget.",
                    notificationDto
                );

                if (sent)
                {
                    user.LastInactivityNotificationSentAt = utcNow;
                    await unitOfWork.CommitAsync();
                    _Logger.LogInformation("Inactivity notification sent to user {UserId}", user.Id);
                }
            }
        }
    }
}
