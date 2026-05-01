using SaltEdge.Interfaces;
using SaltEdge.Models;
using SaltEdge.Models.Connections;

namespace SaltEdge.Services
{
    public class ConnectionsService : HttpClientService, IConnectionsService
    {
        public ConnectionsService(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<ConnectConnectionResponse>> ConnectAsync(ConnectConnectionRequest request, CancellationToken cancellationToken)
        {
            return PostAsync<ConnectConnectionResponse>("connections/connect", request, cancellationToken);
        }

        public Task<ApiResponse<ConnectionResponse>> GetAsync(string connectionId, CancellationToken cancellationToken)
        {
            return GetAsync<ConnectionResponse>($"connections/{connectionId}", cancellationToken);
        }

        public Task<ApiResponse<object>> DeleteAsync(string connectionId, CancellationToken cancellationToken)
        {
            return DeleteAsync<object>($"connections/{connectionId}", cancellationToken);
        }
    }
}
