using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IRefreshTokenUseCase
    {
        Task<CaseResult<AuthResponseDto?>> InvokeAsync(RefreshRequestDto dto);
    }

    public class RefreshTokenUseCase : IRefreshTokenUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ITokenService _TokenService;
        private readonly ILogger<RefreshTokenUseCase> _Logger;

        public RefreshTokenUseCase(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            ILogger<RefreshTokenUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _TokenService = tokenService;
            _Logger = logger;
        }

        public async Task<CaseResult<AuthResponseDto?>> InvokeAsync(RefreshRequestDto dto)
        {
            var result = new CaseResult<AuthResponseDto?>();

            try
            {
                // Check if token exists at all (including revoked)
                var existingToken = await _TokenService.GetRefreshTokenIncludingRevokedAsync(dto.RefreshToken);

                if (existingToken != null && existingToken.RevokedAt != null)
                {
                    // Reuse of a revoked token — potential theft, revoke entire family
                    _Logger.LogWarning("Revoked refresh token reused for user {UserId} — revoking all tokens", existingToken.UserId);
                    await _TokenService.RevokeAllRefreshTokensForUserAsync(existingToken.UserId);
                    result.Successful = false;
                    result.ErrorMessage = "Refresh token has already been used. Please log in again.";
                    return result;
                }

                var validToken = existingToken?.IsActive == true ? existingToken : null;
                if (validToken == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Invalid or expired refresh token.";
                    return result;
                }

                var user = await _UnitOfWork.UserRepository.GetUserById(validToken.UserId);
                if (user == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "User not found.";
                    return result;
                }

                var newRefreshToken = await _TokenService.CreateRefreshTokenAsync(user.Id);
                await _TokenService.RevokeRefreshTokenAsync(validToken, replacedBy: newRefreshToken.Token);

                var accessToken = _TokenService.CreateAccessToken(user);

                result.Successful = true;
                result.Data = new AuthResponseDto(accessToken, newRefreshToken.Token);
                _Logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error refreshing token");
                result.Successful = false;
                result.ErrorMessage = "Something went wrong. Please try again later.";
            }

            return result;
        }
    }
}
