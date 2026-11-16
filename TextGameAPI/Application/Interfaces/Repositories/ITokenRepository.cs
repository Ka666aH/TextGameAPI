using System.Security.Claims;
using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface ITokenRepository
    {
        string GenerateAccessToken(Guid userId, Guid? gameSessionId = null);
        // RawToken уходит только клиенту (cookie/AuthResult); Stored.Token хранит SHA-256,
        // потому что в БД сырой refresh-токен не лежит (notes/AGENTS.md §1b, п.8).
        (string RawToken, RefreshToken Stored) GenerateRefreshToken(Guid userId, string hashedFingerprint);
        ClaimsPrincipal ReadTokenWithoutLifetime(string token);
    }
}