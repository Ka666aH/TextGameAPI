using Microsoft.Extensions.Caching.Distributed;
using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Infrastructure.Cache
{
    public class RedisRepository : ICacheRepository
    {
        private readonly IDistributedCache _cache;

        public RedisRepository(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetAsync(string key, byte[] value, TimeSpan? expiration, CancellationToken ct = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _cache.SetAsync(key, value, options, ct);
        }
        public async Task<byte[]?> GetAsync(string key, CancellationToken ct = default) =>
            await _cache.GetAsync(key, ct);

        public async Task DeleteAsync(string key, CancellationToken ct = default) =>
            await _cache.RemoveAsync(key, ct);
    }
}