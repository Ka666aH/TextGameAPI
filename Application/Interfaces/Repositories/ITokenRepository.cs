using System.Security.Claims;
using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        string GenerateAccessToken(Guid userId, Guid? gameSessionId = null);
        RefreshToken GenerateRefreshToken(Guid userId, string deviceName);
        ClaimsPrincipal ReadTokenWithoutLifetime(string token);
    }
}