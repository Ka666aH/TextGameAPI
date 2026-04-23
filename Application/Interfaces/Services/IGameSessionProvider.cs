using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameSessionProvider
    {
        GameSession GetGameSession();
        Task<GameSession> GetGameSessionAsync(CancellationToken ct = default);
    }
}
