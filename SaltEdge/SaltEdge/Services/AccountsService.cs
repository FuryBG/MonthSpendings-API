using SaltEdge.Interfaces;
using SaltEdge.Models;
using SaltEdge.Models.Accounts;

namespace SaltEdge.Services
{
    public class AccountsService : HttpClientService, IAccountsService
    {
        public AccountsService(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<List<AccountResponse>>> GetAsync(string connectionId, CancellationToken cancellationToken)
        {
            return GetAsync<List<AccountResponse>>($"accounts?connection_id={connectionId}&per_page=1000", cancellationToken);
        }
    }
}
