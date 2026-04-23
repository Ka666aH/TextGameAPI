using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;

namespace TextGame.Application.Services
{
    public class GameSessionProvider : IGameSessionProvider
    {
        private GameSession? _currentSession;
        public GameSession GetGameSession()
        {
            return GetGameSessionAsync().GetAwaiter().GetResult();
        }

        public async Task<GameSession> GetGameSessionAsync(CancellationToken ct = default)
        {
            //here
            _currentSession ??= new GameSession(Guid.Empty);
            return _currentSession;
        }
    }
}