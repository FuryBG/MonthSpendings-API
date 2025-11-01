using Domain;

namespace Application.Interfaces.Repository
{
    public interface IUserRepository
    {
        AppUser AddUser(AppUser user);
    }
}