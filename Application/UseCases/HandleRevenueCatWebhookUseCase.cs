using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IHandleRevenueCatWebhookUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(RevenueCatWebhookPayload payload);
    }

    public class HandleRevenueCatWebhookUseCase : IHandleRevenueCatWebhookUseCase
    {
        private static readonly HashSet<string> ProGrantEvents =
        [
            "INITIAL_PURCHASE", "RENEWAL", "PRODUCT_CHANGE", "RESTORE", "UNCANCELLATION", "TRANSFER"
        ];

        private static readonly HashSet<string> ProRevokeEvents =
        [
            "EXPIRATION"
        ];

        private readonly IUnitOfWork _UnitOfWork;
        private readonly ILogger<HandleRevenueCatWebhookUseCase> _Logger;

        public HandleRevenueCatWebhookUseCase(IUnitOfWork unitOfWork, ILogger<HandleRevenueCatWebhookUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(RevenueCatWebhookPayload payload)
        {
            var result = new CaseResult<bool> { Successful = true, Data = true };
            var ev = payload.Event;

            if (!ProGrantEvents.Contains(ev.Type) && !ProRevokeEvents.Contains(ev.Type))
            {
                _Logger.LogDebug("RevenueCat webhook: ignoring unhandled event type '{EventType}'", ev.Type);
                return result;
            }

            if (!int.TryParse(ev.AppUserId, out int userId))
            {
                _Logger.LogDebug("RevenueCat webhook: skipping non-integer app_user_id '{AppUserId}' for event '{EventType}'", ev.AppUserId, ev.Type);
                return result;
            }

            var user = await _UnitOfWork.UserRepository.GetUserById(userId);
            if (user == null)
            {
                _Logger.LogWarning("RevenueCat webhook: user {UserId} not found for event '{EventType}'", userId, ev.Type);
                return result;
            }

            bool isPro = ProGrantEvents.Contains(ev.Type);
            user.IsPro = isPro;

            var expiresAt = ev.ExpirationAtMs.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(ev.ExpirationAtMs.Value).UtcDateTime
                : DateTime.UtcNow;

            user.Subscriptions.Add(new Subscription
            {
                UserId = user.Id,
                EventType = ev.Type,
                ProductId = ev.ProductId ?? string.Empty,
                Store = ev.Store ?? string.Empty,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
            });

            await _UnitOfWork.CommitAsync();

            _Logger.LogInformation(
                "RevenueCat webhook: user {UserId} IsPro={IsPro} via '{EventType}' — Plan={Plan} Store={Store} ExpiresAt={ExpiresAt}",
                userId, isPro, ev.Type, ev.ProductId, ev.Store, expiresAt);

            return result;
        }
    }
}
