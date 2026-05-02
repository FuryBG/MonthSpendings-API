using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserService> _Logger;

        public UserService(IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _HttpContextAccessor = httpContextAccessor;
            _Logger = logger;
        }

        public int GetUserId()
        {
            var userIdString = _HttpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            int? id = int.TryParse(userIdString, out var parsed) ? parsed : null;

            if (id == null)
            {
                _Logger.LogWarning("UserId claim not found in JWT — unauthorized access attempt");
                throw new UnauthorizedAccessException("Can't find your personal information. Please login.");
            }
            return id.Value;
        }
    }
}
