using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Configuration;

namespace TextGame.Infrastructure.Token.JWT
{
    public class JWTRepository : ITokenRepository
    {
        public (string RawToken, RefreshToken Stored) GenerateRefreshToken(Guid userId, string hashedFingerprint)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var rawToken = Convert.ToBase64String(randomBytes);

            // В БД уходит только хэш; сырой токен вернётся в AuthResult и дальше в cookie.
            var stored = new RefreshToken(
                userId,
                RefreshTokenHash.Of(rawToken),
                DateTime.UtcNow.Add(TimeSettings.RefreshTokenLifetime),
                hashedFingerprint);

            return (rawToken, stored);
        }

        public string GenerateAccessToken(Guid userId, Guid? gameSessionId = null)
        {
            var claims = new List<Claim>
            {
                new(AccessClaims.UserId, userId.ToString())
            };
            if (gameSessionId.HasValue)
                claims.Add(new(AccessClaims.SessionId, gameSessionId.Value.ToString()));
            
            var token = new JwtSecurityToken
            (
                issuer: TokenParameters.Issuer,
                audience: TokenParameters.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSettings.AccessTokenLifetime),
                signingCredentials: new SigningCredentials(JwtKeyProvider.Instance, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal ReadTokenWithoutLifetime(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtKeyProvider.Instance,
                ValidateIssuer = true,
                ValidIssuer = TokenParameters.Issuer, 
                ValidateAudience = true,
                ValidAudience = TokenParameters.Audience,
                ValidateLifetime = false,  //отключаем проверку времени
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenException)
            {
                // Подпись неверна или другие проблемы
                throw new RefreshTokenCompromisedException();
            }
        }
    }
}
