using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private AppDbContext _DbContext { get; set; }
        public UserRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<AppUser?> GetUserById(int userId)
        {
            return await _DbContext.Users
                .Include(u => u.SentBudgetInvites)
                .Include(u => u.ReceivedBudgetInvites)
                .Where(u => u.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<AppUser?> GetUserByGoogleId(string googleId)
        {
            return await _DbContext.Users.Where(u => u.GoogleId == googleId).FirstOrDefaultAsync();
        }

        public async Task<AppUser?> GetUserByEmail(string email)
        {
            return await _DbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        }

        public AppUser AddUser(AppUser user)
        {
            _DbContext.Users.Add(user);
            return user;
        }
    }
}
