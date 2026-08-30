using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface ILoginWithEmailUseCase
    {
        Task<CaseResult<AuthResponseDto?>> InvokeAsync(EmailLoginDto dto);
    }

    public class LoginWithEmailUseCase : ILoginWithEmailUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ITokenService _TokenService;
        private readonly IPasswordService _PasswordService;
        private readonly ILogger<LoginWithEmailUseCase> _Logger;

        public LoginWithEmailUseCase(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPasswordService passwordService,
            ILogger<LoginWithEmailUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _TokenService = tokenService;
            _PasswordService = passwordService;
            _Logger = logger;
        }

        public async Task<CaseResult<AuthResponseDto?>> InvokeAsync(EmailLoginDto dto)
        {
            var result = new CaseResult<AuthResponseDto?>();

            try
            {
                var user = await _UnitOfWork.UserRepository.GetUserByEmail(dto.Email);

                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    // Run a dummy verify to prevent timing-based email enumeration
                    _PasswordService.Verify(dto.Password, _PasswordService.Hash("_dummy_prevent_timing_"));
                    result.Successful = false;
                    result.ErrorMessage = "Invalid email or password.";
                    return result;
                }

                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    var secondsRemaining = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalSeconds;
                    result.Successful = false;
                    result.ErrorMessage = $"Account locked. Try again in {secondsRemaining} seconds.";
                    return result;
                }

                var passwordValid = _PasswordService.Verify(dto.Password, user.PasswordHash);
                if (!passwordValid)
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                        _Logger.LogWarning("User {UserId} locked out after too many failed attempts", user.Id);
                    }
                    await _UnitOfWork.CommitAsync();

                    result.Successful = false;
                    result.ErrorMessage = "Invalid email or password.";
                    return result;
                }

                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                await _UnitOfWork.CommitAsync();

                var accessToken = _TokenService.CreateAccessToken(user);
                var refreshToken = await _TokenService.CreateRefreshTokenAsync(user.Id);

                result.Successful = true;
                result.Data = new AuthResponseDto(accessToken, refreshToken.Token);
                _Logger.LogInformation("User {UserId} logged in with email", user.Id);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error during email login");
                result.Successful = false;
                result.ErrorMessage = "Something went wrong. Please try again later.";
            }

            return result;
        }
    }
}
