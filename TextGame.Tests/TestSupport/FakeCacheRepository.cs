using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// In-memory <see cref="ICacheRepository"/>. Keys and values are kept in separate collections on purpose:
/// a test can park a key in <see cref="Keys"/> and omit its value from <see cref="Values"/>, which is what
/// Redis looks like when a snapshot expires between the key walk and the read.
/// </summary>
internal sealed class FakeCacheRepository : ICacheRepository
{
    /// <summary>Keys the fake reports to <see cref="GetKeysAsync"/>; the pattern filter is a prefix match.</summary>
    public List<string> Keys { get; } = [];

    public Dictionary<string, string> Values { get; } = new(StringComparer.Ordinal);

    public Task SetAsync(string key, string value, TimeSpan? expiration, CancellationToken ct = default)
    {
        if (!Keys.Contains(key)) Keys.Add(key);
        Values[key] = value;
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key, CancellationToken ct = default) =>
        Task.FromResult(Values.TryGetValue(key, out var value) ? value : null);

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        Keys.Remove(key);
        Values.Remove(key);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetKeysAsync(string pattern, CancellationToken ct = default)
    {
        var prefix = pattern.EndsWith('*') ? pattern[..^1] : pattern;
        IReadOnlyList<string> matched = [.. Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal))];
        return Task.FromResult(matched);
    }

    public Task<IReadOnlyDictionary<string, string>> GetManyAsync(IReadOnlyCollection<string> keys, CancellationToken ct = default)
    {
        var output = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in keys)
            if (Values.TryGetValue(key, out var value))
                output[key] = value;

        return Task.FromResult<IReadOnlyDictionary<string, string>>(output);
    }
}
