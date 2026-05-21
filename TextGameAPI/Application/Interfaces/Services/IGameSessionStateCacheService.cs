using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameSessionStateCacheService
    {
        Task SetAsync(Guid gameSessionId, GameSessionState gameSessionState, CancellationToken ct = default);
        Task<GameSessionState?> GetAsync(Guid gameSessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}