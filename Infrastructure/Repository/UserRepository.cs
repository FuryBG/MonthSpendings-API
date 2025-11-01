using Application.Interfaces.Repository;
using Domain;

namespace Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private AppDbContext _DbContext { get; set; }
        public UserRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public AppUser AddUser(AppUser user)
        {
            _DbContext.Users.Add(user);
            return user;
        }
    }
}
