using SaltEdge.Models;
using SaltEdge.Models.Providers;

namespace SaltEdge.Interfaces
{
    public interface IProvidersService
    {
        Task<ApiResponse<List<Provider>>> GetAsync(bool includeSandboxes, CancellationToken cancellationToken);
    }
}
