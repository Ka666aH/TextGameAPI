using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;
using TextGame.Domain.GameText;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.Configuration;
using TextGame.Infrastructure.JSON;

namespace TextGame.Application.Services;

public class AutoSaveService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoSaveService> _logger;

    public AutoSaveService(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<AutoSaveService> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSettings.AutoSaveInterval, stoppingToken);

            try
            {
                await ProcessAutoSaves(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggersText.AutoSaveError);
            }
        }
    }

    private async Task ProcessAutoSaves(CancellationToken ct)
    {
        var redisData = await FetchFromRedis(ct);
        if (redisData.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var saveRepository = scope.ServiceProvider.GetRequiredService<ISaveRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var existingHashes = await saveRepository.GetAutoSaveHashesAsync([.. redisData.Keys], ct);

        var changedIds = new List<Guid>();
        foreach (var (id, (rawJson, hash)) in redisData)
            if (!existingHashes.TryGetValue(id, out var pgHash) || !string.Equals(pgHash, hash, StringComparison.OrdinalIgnoreCase))
                changedIds.Add(id);

        if (changedIds.Count == 0) return;

        var factory = scope.ServiceProvider.GetRequiredService<ISaveFactory>();
        var autoSaves = new List<Save>(changedIds.Count);
        foreach (var id in changedIds)
            autoSaves.Add(factory.CreateAuto(id, StateSerializer.Deserialize(redisData[id].RawJson)));

        await saveRepository.BatchReplaceAutoSavesAsync(changedIds, autoSaves, ct);
        await unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation(LoggersText.AutoSaveSuccess, changedIds.Count);
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

            var idStr = keyStrings[i][CacheParameters.StateKeyPrefix.Length..];
            if (!Guid.TryParse(idStr, out var id)) continue;

            output[id] = (rawJson, ComputeHash(rawJson));
        }
        return output;
    }
    private static string ComputeHash(string rawJson) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawJson)));
}