using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.JSON;

namespace TextGame.Application.Services
{
    public class StateCacheService : IStateCacheService
    {
        private readonly ICacheRepository _cache;

        public StateCacheService(ICacheRepository cacheRepository)
        {
            _cache = cacheRepository;
        }
        public async Task SetAsync(Guid sessionId, State state, CancellationToken ct)
        {
            var key = CacheParameters.StateKeyPrefix + sessionId;
            var value = StateSerializer.Serialize(state);
            await _cache.SetAsync(key, value, CacheParameters.StateLifetime, ct);
        }
        public async Task<State?> GetAsync(Guid sessionId, CancellationToken ct)
        {
            var key = CacheParameters.StateKeyPrefix + sessionId;
            var value = await _cache.GetAsync(key, ct);
            if (value == null) return null;
            return StateSerializer.Deserialize(value);
        }
        public async Task DeleteAsync(Guid sessionId, CancellationToken ct = default)
        {
            var key = CacheParameters.StateKeyPrefix + sessionId;
            await _cache.DeleteAsync(key, ct);
        }
    }
}