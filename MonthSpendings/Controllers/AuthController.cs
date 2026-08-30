using Application.Dto;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterWithEmailUseCase _RegisterWithEmail;
        private readonly ILoginWithEmailUseCase _LoginWithEmail;
        private readonly IRefreshTokenUseCase _RefreshToken;
        private readonly IRevokeRefreshTokenUseCase _RevokeRefreshToken;
        private readonly ILogger<AuthController> _Logger;

        public AuthController(
            IRegisterWithEmailUseCase registerWithEmail,
            ILoginWithEmailUseCase loginWithEmail,
            IRefreshTokenUseCase refreshToken,
            IRevokeRefreshTokenUseCase revokeRefreshToken,
            ILogger<AuthController> logger)
        {
            _RegisterWithEmail = registerWithEmail;
            _LoginWithEmail = loginWithEmail;
            _RefreshToken = refreshToken;
            _RevokeRefreshToken = revokeRefreshToken;
            _Logger = logger;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _RegisterWithEmail.InvokeAsync(dto);
            if (!result.Successful)
            {
                if (result.ErrorMessage?.Contains("already exists") == true)
                    return Conflict(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] EmailLoginDto dto)
        {
            var result = await _LoginWithEmail.InvokeAsync(dto);
            if (!result.Successful)
            {
                if (result.ErrorMessage?.Contains("locked") == true)
                    return StatusCode(StatusCodes.Status423Locked, result.ErrorMessage);
                return Unauthorized(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var result = await _RefreshToken.InvokeAsync(dto);
            if (!result.Successful)
                return Unauthorized(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequestDto dto)
        {
            await _RevokeRefreshToken.InvokeAsync(dto);
            return Ok();
        }
    }
}
