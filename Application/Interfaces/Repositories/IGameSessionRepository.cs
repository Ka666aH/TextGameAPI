using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface IGameSessionRepository
    {
        Task CreateAsync(GameSession gameSession, CancellationToken ct = default);
        Task DeleteAsync(GameSession gameSession, CancellationToken ct = default);
        Task<GameSession?> GetAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<List<GameSession>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    }
}