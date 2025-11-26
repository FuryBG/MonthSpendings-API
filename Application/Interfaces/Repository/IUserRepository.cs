using Domain;

namespace Application.Interfaces.Repository
{
    public interface IUserRepository
    {
        public Task<AppUser?> GetUserById(int userId);
        public Task<AppUser?> GetUserByGoogleId(string googleId);
        AppUser AddUser(AppUser user);
    }
}