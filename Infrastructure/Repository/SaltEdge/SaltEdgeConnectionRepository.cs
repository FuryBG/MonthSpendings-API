using Application.Interfaces.Repository.SaltEdge;
using Domain.SaltEdge;
using Domain.SaltEdge.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository.SaltEdge
{
    public class SaltEdgeConnectionRepository : ISaltEdgeConnectionRepository
    {
        private readonly AppDbContext _dbContext;

        public SaltEdgeConnectionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SaltEdgeConnection> CreateAsync(SaltEdgeConnection connection, CancellationToken cancellationToken)
        {
            await _dbContext.SaltEdgeConnections.AddAsync(connection, cancellationToken);
            return connection;
        }

        public Task<SaltEdgeConnection> UpdateAsync(SaltEdgeConnection connection, CancellationToken cancellationToken)
        {
            _dbContext.SaltEdgeConnections.Update(connection);
            return Task.FromResult(connection);
        }

        public async Task<SaltEdgeConnection?> GetByLocalSessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            return await _dbContext.SaltEdgeConnections
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.LocalSessionId == sessionId, cancellationToken);
        }

        public async Task<SaltEdgeConnection?> GetByConnectionIdAsync(string connectionId, CancellationToken cancellationToken)
        {
            return await _dbContext.SaltEdgeConnections
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId, cancellationToken);
        }

        public async Task<List<SaltEdgeConnection>> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.SaltEdgeConnections
                .Where(c => c.UserId == userId && c.State == SaltEdgeConnectionStatus.Connected)
                .Include(c => c.Accounts)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> DeleteAsync(Expression<Func<SaltEdgeConnection, bool>> expression, CancellationToken cancellationToken)
        {
            return await _dbContext.SaltEdgeConnections.Where(expression).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<List<SaltEdgeConnection>> GetConnectionsForSyncAsync(DateTime threshold, CancellationToken cancellationToken)
        {
            var claimedConnections = await _dbContext.SaltEdgeConnections
                .FromSql($@"
                        UPDATE ""SaltEdgeConnections""
                        SET ""SyncStartedAt"" = NOW()
                        WHERE
                            ""LastSync"" <= {threshold}
                            AND ""State"" = {(int)SaltEdgeConnectionStatus.Connected}
                            AND (""SyncStartedAt"" IS NULL OR ""SyncStartedAt"" < NOW() - INTERVAL '10 minutes')
                        RETURNING *
").ToListAsync(cancellationToken);

            foreach (SaltEdgeConnection connection in claimedConnections)
            {
                await _dbContext.Entry(connection).Collection(c => c.Accounts).LoadAsync(cancellationToken);
            }

            return claimedConnections;
        }

        public Task MarkConnectionsAsSyncedAsync(List<int> connectionIds, DateTime syncedAt, CancellationToken cancellationToken)
        {
            return _dbContext.SaltEdgeConnections
                .Where(c => connectionIds.Contains(c.Id))
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.LastSync, _ => syncedAt)
                    .SetProperty(x => x.SyncStartedAt, _ => null), cancellationToken);
        }
    }
}
