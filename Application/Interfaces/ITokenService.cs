using Domain;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(AppUser user);
        /// <summary>Long-lived token for the phone's wallet listener; its audience only allows posting notification transactions.</summary>
        string CreateWalletIngestToken(AppUser user);
        Task<RefreshToken> CreateRefreshTokenAsync(int userId);
        Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
        Task<RefreshToken?> GetRefreshTokenIncludingRevokedAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedBy = null);
        Task RevokeAllRefreshTokensForUserAsync(int userId);
    }
}
