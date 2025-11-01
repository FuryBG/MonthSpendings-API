using Domain;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
