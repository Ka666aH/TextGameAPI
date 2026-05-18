using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameSessionProvider
    {
        Task<GameSession> GetAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}
