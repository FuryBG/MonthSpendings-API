using Domain;

namespace Application.Interfaces.Repository
{
    public interface INotificationTransactionRepository
    {
        Task<NotificationTransaction> AddAsync(NotificationTransaction transaction, CancellationToken cancellationToken);
        Task<List<NotificationTransaction>> GetUncategorizedByUserAsync(int userId, CancellationToken cancellationToken);
        /// <summary>Same notification already stored (including deleted ones) within ±window of <paramref name="receivedAt"/>.</summary>
        Task<NotificationTransaction?> FindDuplicateAsync(int userId, string? rawTitle, string rawBody, DateTime receivedAt, TimeSpan window, CancellationToken cancellationToken);
        Task<NotificationTransaction?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken);
        Task CategorizeAsync(int transactionId, int spendingId, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
