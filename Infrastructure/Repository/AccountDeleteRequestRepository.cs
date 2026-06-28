using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class AccountDeleteRequestRepository : IAccountDeleteRequestRepository
    {
        private readonly AppDbContext _DbContext;

        public AccountDeleteRequestRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<AccountDeleteRequest?> GetPendingByUserIdAsync(int userId)
        {
            return await _DbContext.AccountDeleteRequests
                .Where(r => r.UserId == userId && r.Status == DeleteRequestStatus.Pending)
                .FirstOrDefaultAsync();
        }

        public AccountDeleteRequest Add(AccountDeleteRequest request)
        {
            _DbContext.AccountDeleteRequests.Add(request);
            return request;
        }
    }
}
