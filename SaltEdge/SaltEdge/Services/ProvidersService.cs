using SaltEdge.Interfaces;
using SaltEdge.Models;
using SaltEdge.Models.Providers;

namespace SaltEdge.Services
{
    public class ProvidersService : HttpClientService, IProvidersService
    {
        public ProvidersService(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<List<Provider>>> GetAsync(bool includeSandboxes, CancellationToken cancellationToken)
        {
            return GetAsync<List<Provider>>($"providers?include_sandboxes={includeSandboxes.ToString().ToLowerInvariant()}&exclude_inactive=true&per_page=100", cancellationToken);
        }
    }
}
