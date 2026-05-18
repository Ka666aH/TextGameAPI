using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameSessionCacheService
    {
        Task SetAsync(GameSession gameSession, CancellationToken ct = default);
        Task<GameSession?> GetAsync(Guid gameSessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}