using StackExchange.Redis;
using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Infrastructure.Cache
{
    public class RedisRepository : ICacheRepository
    {
        // Сколько ключей просматривает сервер за один вызов SCAN. Это подсказка, а не лимит:
        // страница может быть и пустой, и больше COUNT — поэтому цикл идёт до курсора 0.
        private const int ScanPageSize = 1000;

        // Страховка от бесконечного цикла в фоновом сервисе: 10k страниц по 1000 = 10 млн ключей.
        private const int MaxScanPages = 10_000;

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

        public async Task<IReadOnlyList<string>> GetKeysAsync(string pattern, CancellationToken ct = default)
        {
            var keys = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            long cursor = 0;
            var pages = 0;

            do
            {
                ct.ThrowIfCancellationRequested();

                // SCAN, а не KEYS: KEYS блокирует весь инстанс на O(N) ключей и на большой базе
                // подвешивает всех клиентов, включая игровые запросы. SCAN отдаёт по странице.
                var reply = (RedisResult[]?)await _db.ExecuteAsync(
                    "SCAN", cursor, "MATCH", pattern, "COUNT", ScanPageSize);

                if (reply is null || reply.Length < 2) break;
                if (!long.TryParse((string?)reply[0], out cursor)) break;

                // Дубликаты между страницами — штатное поведение SCAN, дедуплицируем.
                var batch = (RedisResult[]?)reply[1];
                if (batch is not null)
                    foreach (var item in batch)
                        if ((string?)item is { } key && seen.Add(key))
                            keys.Add(key);
            }
            while (cursor != 0 && ++pages < MaxScanPages);

            return keys;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetManyAsync(IReadOnlyCollection<string> keys, CancellationToken ct = default)
        {
            var output = new Dictionary<string, string>(keys.Count, StringComparer.Ordinal);
            if (keys.Count == 0) return output;

            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _db.StringGetAsync(redisKeys);

            for (var i = 0; i < redisKeys.Length; i++)
                if ((string?)values[i] is { } value)
                    output[(string)redisKeys[i]] = value;

            return output;
        }
    }
}
