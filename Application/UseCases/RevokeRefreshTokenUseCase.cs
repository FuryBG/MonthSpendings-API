using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IRevokeRefreshTokenUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(RevokeRequestDto dto);
    }

    public class RevokeRefreshTokenUseCase : IRevokeRefreshTokenUseCase
    {
        private readonly ITokenService _TokenService;
        private readonly ILogger<RevokeRefreshTokenUseCase> _Logger;

        public RevokeRefreshTokenUseCase(
            ITokenService tokenService,
            ILogger<RevokeRefreshTokenUseCase> logger)
        {
            _TokenService = tokenService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(RevokeRequestDto dto)
        {
            var result = new CaseResult<bool>();

            try
            {
                var token = await _TokenService.GetValidRefreshTokenAsync(dto.RefreshToken);
                if (token != null)
                {
                    await _TokenService.RevokeRefreshTokenAsync(token);
                    _Logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);
                }

                result.Successful = true;
                result.Data = true;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error revoking token");
                result.Successful = false;
                result.ErrorMessage = "Something went wrong.";
            }

            return result;
        }
    }
}
