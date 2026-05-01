using Application.Interfaces.Repository.SaltEdge;
using Domain.SaltEdge;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.SaltEdge
{
    public class SaltEdgeCustomerRepository : ISaltEdgeCustomerRepository
    {
        private readonly AppDbContext _dbContext;

        public SaltEdgeCustomerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SaltEdgeCustomer?> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.SaltEdgeCustomers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }

        public async Task<SaltEdgeCustomer> CreateAsync(SaltEdgeCustomer customer, CancellationToken cancellationToken)
        {
            await _dbContext.SaltEdgeCustomers.AddAsync(customer, cancellationToken);
            return customer;
        }

        public Task<SaltEdgeCustomer> UpdateAsync(SaltEdgeCustomer customer, CancellationToken cancellationToken)
        {
            _dbContext.SaltEdgeCustomers.Update(customer);
            return Task.FromResult(customer);
        }
    }
}
