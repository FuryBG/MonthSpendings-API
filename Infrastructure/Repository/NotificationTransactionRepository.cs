using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class NotificationTransactionRepository : INotificationTransactionRepository
    {
        private readonly AppDbContext _DbContext;

        public NotificationTransactionRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<NotificationTransaction> AddAsync(NotificationTransaction transaction, CancellationToken cancellationToken)
        {
            await _DbContext.NotificationTransactions.AddAsync(transaction, cancellationToken);
            return transaction;
        }

        public Task<List<NotificationTransaction>> GetUncategorizedByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return _DbContext.NotificationTransactions
                .Where(t => t.UserId == userId && !t.Categorized)
                .OrderByDescending(t => t.ReceivedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<NotificationTransaction?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken)
        {
            return _DbContext.NotificationTransactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
        }

        public async Task CategorizeAsync(int transactionId, int spendingId, CancellationToken cancellationToken)
        {
            await _DbContext.NotificationTransactions
                .Where(t => t.Id == transactionId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Categorized, true)
                    .SetProperty(t => t.SpendingId, spendingId),
                    cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            await _DbContext.NotificationTransactions
                .IgnoreQueryFilters()
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsDeleted, true), cancellationToken);
        }
    }
}
