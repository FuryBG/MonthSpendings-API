using SaltEdge.Models;
using SaltEdge.Models.Connections;

namespace SaltEdge.Interfaces
{
    public interface IConnectionsService
    {
        Task<ApiResponse<ConnectConnectionResponse>> ConnectAsync(ConnectConnectionRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<ConnectionResponse>> GetAsync(string connectionId, CancellationToken cancellationToken);
        Task<ApiResponse<object>> DeleteAsync(string connectionId, CancellationToken cancellationToken);
    }
}
