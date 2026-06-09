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

        public async Task<State> GetAsync(Guid sessionId, CancellationToken ct = default)
        {
            var cached = await _cache.GetAsync(sessionId, ct);
            if (cached != null) return cached;
            var save = await _saveRepository.GetLastSaveAsync(sessionId, ct) ?? throw new SaveNotFoundException();
            var state = save.State;
            try { await _cache.SetAsync(sessionId, state, ct); } catch { }
            return state;
        }
    }
}