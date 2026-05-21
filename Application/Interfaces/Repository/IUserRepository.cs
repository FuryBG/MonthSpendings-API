using Domain;

namespace Application.Interfaces.Repository
{
    public interface IUserRepository
    {
        public Task<AppUser?> GetUserById(int userId);
        public Task<AppUser?> GetUserByGoogleId(string googleId);
        public Task<AppUser?> GetUserByEmail(string email);
        AppUser AddUser(AppUser user);
        Task<List<AppUser>> GetAllUsersWithNotificationTokenAsync();
    }
}