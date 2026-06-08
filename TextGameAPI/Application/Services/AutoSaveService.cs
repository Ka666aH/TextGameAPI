using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.JSON;

namespace TextGame.Application.Services;

public class AutoSaveService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public AutoSaveService(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                await ProcessAutoSaves(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-save error:{ex.Message}");
            }
        }
    }

    private async Task ProcessAutoSaves(CancellationToken ct)
    {
        var redisData = await FetchFromRedis(ct);
        if (redisData.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISaveRepository>();

        var existingHashes = await repo.GetAutoSaveHashesAsync([.. redisData.Keys], ct);

        var changedIds = new List<Guid>();
        foreach (var (id, (rawJson, hash)) in redisData)
            if (!existingHashes.TryGetValue(id, out var pgHash) || !string.Equals(pgHash, hash, StringComparison.OrdinalIgnoreCase))
                changedIds.Add(id);

        if (changedIds.Count == 0) return;

        var factory = scope.ServiceProvider.GetRequiredService<ISaveFactory>();
        var autoSaves = new List<Save>(changedIds.Count);
        foreach (var id in changedIds)
            autoSaves.Add(factory.CreateAutoGameSessionSave(id, StateSerializer.Deserialize(redisData[id].RawJson)));

        await repo.BatchReplaceAutoSavesAsync(changedIds, autoSaves, ct);
        Console.WriteLine($"Auto-saved {changedIds.Count} sessions");
    }

    private async Task<Dictionary<Guid, (string RawJson, string Hash)>> FetchFromRedis(CancellationToken ct)
    {
        var db = _redis.GetDatabase();

        var result = (RedisResult[]?)await db.ExecuteAsync("KEYS", "*");
        if (result == null) return [];

        var keyStrings = result.Select(k => (string)k!).ToList();
        if (keyStrings.Count == 0) return [];
        var redisKeys = keyStrings.Select(k => (RedisKey)k).ToArray();
        var values = await db.StringGetAsync(redisKeys);

        var output = new Dictionary<Guid, (string, string)>(keyStrings.Count);
        for (var i = 0; i < keyStrings.Count; i++)
        {
            var rawJson = (string?)values[i];
            if (rawJson == null) continue;

            var idStr = keyStrings[i][CacheParameters.GameSessionStateKeyPrefix.Length..];
            if (!Guid.TryParse(idStr, out var id)) continue;

            output[id] = (rawJson, ComputeHash(rawJson));
        }
        return output;
    }
    private static string ComputeHash(string rawJson) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawJson)));
}