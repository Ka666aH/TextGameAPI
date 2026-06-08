using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class SessionProvider : ISessionProvider
    {
        private readonly ISaveRepository _saveRepository;
        private readonly IStateCacheService _cache;

        public SessionProvider(ISaveRepository saveRepository, IStateCacheService stateCacheService)
        {
            _saveRepository = saveRepository;
            _cache = stateCacheService;
        }

        public async Task<State> GetAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var cached = await _cache.GetAsync(gameSessionId, ct);
            if (cached != null) return cached;
            var gameSessionSave = await _saveRepository.GetLastSaveAsync(gameSessionId, ct) ?? throw new SaveNotFoundException();
            var gameSessionState = gameSessionSave.State;
            try { await _cache.SetAsync(gameSessionId, gameSessionState, ct); } catch { }
            return gameSessionState;
        }
    }
}