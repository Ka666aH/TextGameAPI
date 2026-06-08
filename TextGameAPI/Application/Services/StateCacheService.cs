using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.JSON;

namespace TextGame.Application.Services
{
    public class StateCacheService : IStateCacheService
    {
        private readonly ICacheRepository _cacheRepository;

        public StateCacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }
        public async Task SetAsync(Guid gameSessionId, State gameSessionState, CancellationToken ct)
        {
            var key = CacheParameters.GameSessionStateKeyPrefix + gameSessionId;
            var value = StateSerializer.Serialize(gameSessionState);
            await _cacheRepository.SetAsync(key, value, CacheParameters.GameSessionLifetime, ct);
        }
        public async Task<State?> GetAsync(Guid gameSessionId, CancellationToken ct)
        {
            var key = CacheParameters.GameSessionStateKeyPrefix + gameSessionId;
            var value = await _cacheRepository.GetAsync(key, ct);
            if (value == null) return null;
            return StateSerializer.Deserialize(value);
        }
        public async Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var key = CacheParameters.GameSessionStateKeyPrefix + gameSessionId;
            await _cacheRepository.DeleteAsync(key, ct);
        }
    }
}