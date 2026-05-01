using Application.Interfaces.Repository.SaltEdge;
using Domain.SaltEdge;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.SaltEdge
{
    public class SaltEdgeAccountRepository : ISaltEdgeAccountRepository
    {
        private readonly AppDbContext _dbContext;

        public SaltEdgeAccountRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> DeleteByConnectionDbIdAsync(int connectionDbId, CancellationToken cancellationToken)
        {
            return _dbContext.SaltEdgeAccounts.Where(a => a.ConnectionDbId == connectionDbId).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<List<SaltEdgeAccount>> AddRangeAsync(List<SaltEdgeAccount> accounts, CancellationToken cancellationToken)
        {
            await _dbContext.SaltEdgeAccounts.AddRangeAsync(accounts, cancellationToken);
            return accounts;
        }
    }
}
