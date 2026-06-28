using Domain;

namespace Application.Interfaces.Repository
{
    public interface IAccountDeleteRequestRepository
    {
        Task<AccountDeleteRequest?> GetPendingByUserIdAsync(int userId);
        AccountDeleteRequest Add(AccountDeleteRequest request);
    }
}
