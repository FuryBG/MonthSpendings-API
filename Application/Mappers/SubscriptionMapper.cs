using Application.Dto;
using Domain;

namespace Application.Mappers
{
    public static class SubscriptionMapper
    {
        public static SubscriptionDto ToDto(this Subscription subscription) => new()
        {
            EventType = subscription.EventType,
            ProductId = subscription.ProductId,
            Store = subscription.Store,
            ExpiresAt = subscription.ExpiresAt,
            CreatedAt = subscription.CreatedAt,
        };
    }
}
