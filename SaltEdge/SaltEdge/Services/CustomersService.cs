using SaltEdge.Interfaces;
using SaltEdge.Models;
using SaltEdge.Models.Customers;

namespace SaltEdge.Services
{
    public class CustomersService : HttpClientService, ICustomersService
    {
        public CustomersService(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<CustomerResponse>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            return PostAsync<CustomerResponse>("customers", request, cancellationToken);
        }
    }
}
