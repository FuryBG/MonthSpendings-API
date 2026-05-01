using SaltEdge.Models;
using SaltEdge.Models.Accounts;

namespace SaltEdge.Interfaces
{
    public interface IAccountsService
    {
        Task<ApiResponse<List<AccountResponse>>> GetAsync(string connectionId, CancellationToken cancellationToken);
    }
}
