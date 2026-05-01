using SaltEdge.Models;
using SaltEdge.Models.Customers;

namespace SaltEdge.Interfaces
{
    public interface ICustomersService
    {
        Task<ApiResponse<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    }
}
