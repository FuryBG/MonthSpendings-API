using Application.Dto;
using Domain;

namespace Application.Mappers
{
    public static class NotificationTransactionMapper
    {
        public static NotificationTransactionDto ToDto(this NotificationTransaction transaction)
        {
            return new NotificationTransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                MerchantName = transaction.MerchantName,
                CardLast4 = transaction.CardLast4,
                ReceivedAt = transaction.ReceivedAt,
                Categorized = transaction.Categorized,
            };
        }
    }
}
