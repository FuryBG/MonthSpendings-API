using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services
{
    public interface IUserService
    {
        int GetUserId();
    }

    public class UserService : IUserService
    {
        private IHttpContextAccessor _HttpContextAccessor { get; set; }

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _HttpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            var userIdString = _HttpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            int? id = int.TryParse(userIdString, out var parsed) ? parsed : null;

            if (id == null)
            {
                throw new UnauthorizedAccessException("Can't find your personal information. Please login.");
            }
            return id.Value;
        }
    }
}
