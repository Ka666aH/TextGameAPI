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
    private readonly ICacheRepository _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoSaveService> _logger;

    public AutoSaveService(
        ICacheRepository cacheRepository,
        IServiceScopeFactory scopeFactory,
        ILogger<AutoSaveService> logger)
    {
        _cache = cacheRepository;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(LoggersText.AutoSaveStarted);
            try
            {
                await ProcessAutoSaves(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LoggersText.AutoSaveError);
            }
            await Task.Delay(TimeSettings.AutoSaveInterval, stoppingToken);
        }
    }

    private async Task ProcessAutoSaves(CancellationToken ct)
    {
        var redisData = await FetchFromRedis(_cache, ct);
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

    internal static async Task<Dictionary<Guid, (string RawJson, string Hash)>> FetchFromRedis(
        ICacheRepository cache, CancellationToken ct)
    {
        var keys = await cache.GetKeysAsync($"{CacheParameters.StateKeyPrefix}*", ct);
        if (keys.Count == 0) return [];

        // Ключ может не совпасть с паттерном, если кэш переиспользуется под другие данные:
        // сначала проверяем префикс, только потом режем строку — иначе на коротком ключе
        // вылетал ArgumentOutOfRangeException.
        var ids = new Dictionary<string, Guid>(keys.Count, StringComparer.Ordinal);
        foreach (var key in keys)
            if (TryParseStateKey(key, out var id))
                ids[key] = id;
        if (ids.Count == 0) return [];

        var values = await cache.GetManyAsync([.. ids.Keys], ct);

        var output = new Dictionary<Guid, (string, string)>(values.Count);
        foreach (var (key, rawJson) in values)
            output[ids[key]] = (rawJson, ComputeHash(rawJson));

        return output;
    }

    internal static bool TryParseStateKey(string key, out Guid id)
    {
        id = Guid.Empty;
        if (!key.StartsWith(CacheParameters.StateKeyPrefix, StringComparison.Ordinal)) return false;
        return Guid.TryParse(key.AsSpan(CacheParameters.StateKeyPrefix.Length), out id);
    }

    private static string ComputeHash(string rawJson) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawJson)));
}
