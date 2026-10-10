using Application.Resources;
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

            var existingToken = await _TokenService.GetRefreshTokenIncludingRevokedAsync(dto.RefreshToken);

            if (existingToken != null && existingToken.RevokedAt != null)
            {
                _Logger.LogWarning("Revoked refresh token reused for user {UserId} — revoking all tokens", existingToken.UserId);
                await _TokenService.RevokeAllRefreshTokensForUserAsync(existingToken.UserId);
                return CaseResult<AuthResponseDto?>.Error(Messages.AuthRefreshTokenReused, ErrorType.Unauthorized);
            }

            var validToken = existingToken?.IsActive == true ? existingToken : null;
            if (validToken == null)
            {
                _Logger.LogInformation("Token refresh failed: refresh token unknown or expired");
                return CaseResult<AuthResponseDto?>.Error(Messages.AuthRefreshTokenInvalid, ErrorType.Unauthorized);
            }

            var user = await _UnitOfWork.UserRepository.GetUserById(validToken.UserId);
            if (user == null)
            {
                _Logger.LogWarning("Token refresh failed: refresh token belongs to missing user {UserId}", validToken.UserId);
                return CaseResult<AuthResponseDto?>.Error(Messages.UserNotFound);
            }

            var newRefreshToken = await _TokenService.CreateRefreshTokenAsync(user.Id);
            await _TokenService.RevokeRefreshTokenAsync(validToken, replacedBy: newRefreshToken.Token);

            var accessToken = _TokenService.CreateAccessToken(user);

            result.Successful = true;
            result.Data = new AuthResponseDto(accessToken, newRefreshToken.Token);
            _Logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);

            return result;
        }
    }
}
