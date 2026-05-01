using Domain.SaltEdge;

namespace Application.Interfaces.Repository.SaltEdge
{
    public interface ISaltEdgeAccountRepository
    {
        Task<int> DeleteByConnectionDbIdAsync(int connectionDbId, CancellationToken cancellationToken);
        Task<List<SaltEdgeAccount>> AddRangeAsync(List<SaltEdgeAccount> accounts, CancellationToken cancellationToken);
    }
}
