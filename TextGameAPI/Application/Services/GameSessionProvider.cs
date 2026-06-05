using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GameSessionProvider : IGameSessionProvider
    {
        private readonly IGameSessionSaveRepository _gameSessionSaveRepository;
        private readonly IGameSessionStateCacheService _gameSessionCacheService;

        public GameSessionProvider(IGameSessionSaveRepository gameSessionSaveRepository, IGameSessionStateCacheService gameSessionCacheService)
        {
            _gameSessionSaveRepository = gameSessionSaveRepository;
            _gameSessionCacheService = gameSessionCacheService;
        }

        public async Task<GameSessionState> GetAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var cached = await _gameSessionCacheService.GetAsync(gameSessionId, ct);
            if (cached != null) return cached;
            var gameSessionSave = await _gameSessionSaveRepository.GetLastSaveAsync(gameSessionId, ct) ?? throw new GameSessionSaveNotFoundException();
            var gameSessionState = gameSessionSave.State;
            try { await _gameSessionCacheService.SetAsync(gameSessionId, gameSessionState, ct); } catch { }
            return gameSessionState;
        }
    }
}