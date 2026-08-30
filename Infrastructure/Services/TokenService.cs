using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, AppDbContext db, ILogger<TokenService> logger)
        {
            _config = config;
            _db = db;
            _logger = logger;
        }

        public string CreateAccessToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            _logger.LogInformation("Access token issued for user {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
        {
            var token = new RefreshToken
            {
                Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
            };
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshToken?> GetValidRefreshTokenAsync(string token)
        {
            return await _db.RefreshTokens
                .Where(t => t.Token == token && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenIncludingRevokedAsync(string token)
        {
            return await _db.RefreshTokens
                .Where(t => t.Token == token)
                .FirstOrDefaultAsync();
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedBy = null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = replacedBy;
            await _db.SaveChangesAsync();
        }

        public async Task RevokeAllRefreshTokensForUserAsync(int userId)
        {
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            foreach (var t in tokens)
                t.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
