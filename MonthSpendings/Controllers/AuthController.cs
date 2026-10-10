using Application.Resources;
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

        public AuthController(
            IRegisterWithEmailUseCase registerWithEmail,
            ILoginWithEmailUseCase loginWithEmail,
            IRefreshTokenUseCase refreshToken,
            IRevokeRefreshTokenUseCase revokeRefreshToken)
        {
            _RegisterWithEmail = registerWithEmail;
            _LoginWithEmail = loginWithEmail;
            _RefreshToken = refreshToken;
            _RevokeRefreshToken = revokeRefreshToken;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _RegisterWithEmail.InvokeAsync(dto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
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
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var result = await _RefreshToken.InvokeAsync(dto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
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
