using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GameSessionProvider : IGameSessionProvider
    {
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IGameSessionCacheService _gameSessionCacheService;

        public GameSessionProvider(IGameSessionRepository gameSessionRepository, IGameSessionCacheService gameSessionCacheService)
        {
            _gameSessionRepository = gameSessionRepository;
            _gameSessionCacheService = gameSessionCacheService;
        }

        public async Task<GameSession> GetAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var cached = await _gameSessionCacheService.GetAsync(gameSessionId, ct);
            if (cached != null) return cached;
            var gameSession = await _gameSessionRepository.GetAsync(gameSessionId, ct) ?? throw new GameSessionNotFoundException();
            try { await _gameSessionCacheService.SetAsync(gameSession, ct); } catch { }
            return gameSession;
        }
    }
}