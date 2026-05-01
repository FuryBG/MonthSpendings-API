using Application.Interfaces.Repository.SaltEdge;
using Domain.SaltEdge;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.SaltEdge
{
    public class SaltEdgeTransactionRepository : ISaltEdgeTransactionRepository
    {
        private readonly AppDbContext _dbContext;

        public SaltEdgeTransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SaltEdgeTransaction> AddTransactionAsync(SaltEdgeTransaction transaction, CancellationToken cancellationToken)
        {
            await _dbContext.SaltEdgeTransactions.AddAsync(transaction, cancellationToken);
            return transaction;
        }

        public async Task<HashSet<string>> GetExistingTransactionIdsAsync(List<string> transactionIds, CancellationToken cancellationToken)
        {
            List<string> existingIds = await _dbContext.SaltEdgeTransactions
                .Where(t => transactionIds.Contains(t.TransactionId))
                .Select(t => t.TransactionId)
                .ToListAsync(cancellationToken);

            return existingIds.ToHashSet();
        }
    }
}
