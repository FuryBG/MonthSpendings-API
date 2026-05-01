using SaltEdge.Interfaces;
using SaltEdge.Models;
using SaltEdge.Models.Transactions;

namespace SaltEdge.Services
{
    public class TransactionsService : HttpClientService, ITransactionsService
    {
        public TransactionsService(HttpClient httpClient) : base(httpClient)
        {
        }

        public Task<ApiResponse<List<TransactionResponse>>> GetAsync(string connectionId, string? fromId, bool pending, CancellationToken cancellationToken)
        {
            string fromIdQuery = string.IsNullOrWhiteSpace(fromId) ? string.Empty : $"&from_id={Uri.EscapeDataString(fromId)}";
            return GetAsync<List<TransactionResponse>>($"transactions?connection_id={connectionId}&pending={pending.ToString().ToLowerInvariant()}&per_page=1000{fromIdQuery}", cancellationToken);
        }
    }
}
