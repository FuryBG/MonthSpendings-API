using Domain;

namespace Application.Interfaces.Repository
{
    public interface INotificationTransactionRepository
    {
        Task<NotificationTransaction> AddAsync(NotificationTransaction transaction, CancellationToken cancellationToken);
        Task<List<NotificationTransaction>> GetUncategorizedByUserAsync(int userId, CancellationToken cancellationToken);
        Task<NotificationTransaction?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken);
        Task CategorizeAsync(int transactionId, int spendingId, CancellationToken cancellationToken);
    }
}
