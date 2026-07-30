using StackExchange.Redis;
using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Infrastructure.Cache
{
    public class RedisRepository : ICacheRepository
    {
        private readonly IDatabase _db;

        public RedisRepository(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiration, CancellationToken ct = default)
        {
            if (expiration.HasValue)
                await _db.StringSetAsync(key, value, expiration.Value);
            else
                await _db.StringSetAsync(key, value);
        }
        public async Task<string?> GetAsync(string key, CancellationToken ct = default) =>
            await _db.StringGetAsync(key);

        public async Task DeleteAsync(string key, CancellationToken ct = default) =>
            await _db.KeyDeleteAsync(key);
    }
}