using Domain.SaltEdge;

namespace Application.Interfaces.Repository.SaltEdge
{
    public interface ISaltEdgeTransactionRepository
    {
        Task<SaltEdgeTransaction> AddTransactionAsync(SaltEdgeTransaction transaction, CancellationToken cancellationToken);
        Task<HashSet<string>> GetExistingTransactionIdsAsync(List<string> transactionIds, CancellationToken cancellationToken);
    }
}
