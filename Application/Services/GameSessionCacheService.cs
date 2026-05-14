using System.Text.Json;
using System.Text.Json.Serialization;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.Cache;

namespace TextGame.Application.Services
{
    public class GameSessionCacheService : IGameSessionCacheService
    {
        private readonly ICacheRepository _cacheRepository;

        private const string KeyTemplate = "gameSession:{0}";
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public GameSessionCacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        public async Task SetAsync(GameSession gameSession, CancellationToken ct = default)
        {
            var key = string.Format(KeyTemplate, gameSession.Id);
            var value = JsonSerializer.SerializeToUtf8Bytes<GameSession>(gameSession, _jsonOptions);
            await _cacheRepository.SetAsync(key, value, CacheParameters.GameSessionLifetime, ct);
        }
        public async Task<GameSession?> GetAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var key = string.Format(KeyTemplate, gameSessionId);
            var value = await _cacheRepository.GetAsync(key, ct);
            if (value == null) return null;
            return JsonSerializer.Deserialize<GameSession>(value, _jsonOptions);
        }
        public async Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            var key = string.Format(KeyTemplate, gameSessionId);
            await _cacheRepository.DeleteAsync(key, ct);
        }
    }
}