using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.JSON;

namespace TextGame.Application.Services
{
    public class GameSessionStateCacheService : IGameSessionStateCacheService
    {
        private readonly ICacheRepository _cacheRepository;

        private const string KeyTemplate = "GameSessionState:{0}";

        public GameSessionStateCacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }
        public async Task SetAsync(Guid gameSessionId, GameSessionState gameSessionState, CancellationToken ct)
        {
            var key = string.Format(KeyTemplate, gameSessionId);
            var value = GameSessionStateSerializer.Serialize(gameSessionState);
            await _cacheRepository.SetAsync(key, value, CacheParameters.GameSessionLifetime, ct);
        }
        public async Task<GameSessionState?> GetAsync(Guid gameSessionId, CancellationToken ct)
        {
            var key = string.Format(KeyTemplate, gameSessionId);
            var value = await _cacheRepository.GetAsync(key, ct);
            if (value == null) return null;
            return GameSessionStateSerializer.Deserialize(value);
        }
        public async Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var key = string.Format(KeyTemplate, gameSessionId);
            await _cacheRepository.DeleteAsync(key, ct);
        }
    }
}