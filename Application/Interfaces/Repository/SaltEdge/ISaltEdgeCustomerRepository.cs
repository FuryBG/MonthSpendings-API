using Domain.SaltEdge;

namespace Application.Interfaces.Repository.SaltEdge
{
    public interface ISaltEdgeCustomerRepository
    {
        Task<SaltEdgeCustomer?> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task<SaltEdgeCustomer> CreateAsync(SaltEdgeCustomer customer, CancellationToken cancellationToken);
        Task<SaltEdgeCustomer> UpdateAsync(SaltEdgeCustomer customer, CancellationToken cancellationToken);
    }
}
