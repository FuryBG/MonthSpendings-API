using SaltEdge.Models;
using SaltEdge.Models.Transactions;

namespace SaltEdge.Interfaces
{
    public interface ITransactionsService
    {
        Task<ApiResponse<List<TransactionResponse>>> GetAsync(string connectionId, string? fromId, bool pending, CancellationToken cancellationToken);
    }
}
