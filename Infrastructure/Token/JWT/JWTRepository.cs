using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Token.JWT
{
    public class JWTRepository : ITokenRepository
    {
        public RefreshToken GenerateRefreshToken(Guid userId, string deviceName)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes);

            return new RefreshToken(
                userId,
                token,
                DateTime.UtcNow.Add(Parameters.RefreshTokenLifetime),
                deviceName);
        }

        public string GenerateAccessToken(Guid userId, Guid? gameSessionId = null)
        {
            var claims = new List<Claim>
            {
                new(AccessClaims.UserId, userId.ToString())
            };
            if (gameSessionId.HasValue)
                claims.Add(new(AccessClaims.GameSessionId, gameSessionId.Value.ToString()));
            
            var token = new JwtSecurityToken
            (
                issuer: Parameters.Issuer,
                audience: Parameters.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(Parameters.AccessTokenLifetime),
                signingCredentials: new SigningCredentials(JwtKeyProvider.Instance, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
