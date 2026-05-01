using Domain.SaltEdge;
using System.Linq.Expressions;

namespace Application.Interfaces.Repository.SaltEdge
{
    public interface ISaltEdgeConnectionRepository
    {
        Task<SaltEdgeConnection> CreateAsync(SaltEdgeConnection connection, CancellationToken cancellationToken);
        Task<SaltEdgeConnection> UpdateAsync(SaltEdgeConnection connection, CancellationToken cancellationToken);
        Task<SaltEdgeConnection?> GetByLocalSessionIdAsync(Guid sessionId, CancellationToken cancellationToken);
        Task<SaltEdgeConnection?> GetByConnectionIdAsync(string connectionId, CancellationToken cancellationToken);
        Task<List<SaltEdgeConnection>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task<int> DeleteAsync(Expression<Func<SaltEdgeConnection, bool>> expression, CancellationToken cancellationToken);
        Task<List<SaltEdgeConnection>> GetConnectionsForSyncAsync(DateTime threshold, CancellationToken cancellationToken);
        Task MarkConnectionsAsSyncedAsync(List<int> connectionIds, DateTime syncedAt, CancellationToken cancellationToken);
    }
}
