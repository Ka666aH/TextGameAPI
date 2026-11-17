using TextGame.Application.Services;
using TextGame.Infrastructure.Cache;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Application;

/// <summary>
/// The auto-save sweep walks every State snapshot in Redis and compares its hash with Postgres.
/// It used to run <c>KEYS *</c> — a blocking O(N) call that stalls every other Redis client — and sliced
/// the key offset by the prefix length without checking the prefix first.
/// These tests pin what the sweep must keep: a session id per key, a content hash, and no exceptions
/// from anything the cache hands back.
/// </summary>
public class AutoSaveServiceTests
{
    private const string StatePrefix = CacheParameters.StateKeyPrefix;

    [Fact]
    public async Task FetchFromRedis_WhenCacheIsEmpty_ReturnsEmptyDictionary()
    {
        // Arrange
        var cache = new FakeCacheRepository();

        // Act
        var result = await AutoSaveService.FetchFromRedis(cache, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FetchFromRedis_WhenSnapshotsExist_ReturnsEachSessionIdWithItsContentHash()
    {
        // Arrange
        var cache = new FakeCacheRepository();
        var firstSession = Guid.NewGuid();
        var secondSession = Guid.NewGuid();
        await cache.SetAsync($"{StatePrefix}{firstSession}", "{}", TimeSpan.FromMinutes(1));
        await cache.SetAsync($"{StatePrefix}{secondSession}", "{\"Coins\":1}", TimeSpan.FromMinutes(1));

        // Act
        var result = await AutoSaveService.FetchFromRedis(cache, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[firstSession].RawJson.Should().Be("{}");
        result[firstSession].Hash.Should().Be("44136FA355B3678A1146AD16F7E8649E94FB4FC21FE77E8310C060F61CAAFF8A");
        result[secondSession].RawJson.Should().Be("{\"Coins\":1}");
        result[secondSession].Hash.Should().Be("207216DF913F85FD3F6DAA6D779E1AAF2BCE51DB15B22E82A0E2E2F21F943DCA");
    }

    [Fact]
    public async Task FetchFromRedis_WhenSnapshotExpiresBetweenWalkAndRead_SkipsThatSession()
    {
        // Arrange
        var cache = new FakeCacheRepository();
        var alive = Guid.NewGuid();
        var expired = Guid.NewGuid();
        cache.Keys.Add($"{StatePrefix}{alive}");
        cache.Keys.Add($"{StatePrefix}{expired}");
        cache.Values[$"{StatePrefix}{alive}"] = "{}";

        // Act
        var result = await AutoSaveService.FetchFromRedis(cache, CancellationToken.None);

        // Assert
        result.Should().ContainKey(alive);
        result.Should().NotContainKey(expired);
    }

    [Fact]
    public async Task FetchFromRedis_WhenSameSessionAppearsTwice_SavesItOnce()
    {
        // Arrange
        var cache = new FakeCacheRepository();
        var sessionId = Guid.NewGuid();
        string key = $"{StatePrefix}{sessionId}";
        cache.Keys.Add(key);
        cache.Keys.Add(key);
        cache.Values[key] = "{}";

        // Act
        var result = await AutoSaveService.FetchFromRedis(cache, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[sessionId].RawJson.Should().Be("{}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("State:")]
    [InlineData("State:not-a-guid")]
    [InlineData("State:1")]
    [InlineData("Other:e5a1d6b4-1df0-4c40-8c1a-6b9c1ac1a1a1")]
    [InlineData("ab")]
    public void TryParseStateKey_WhenKeyIsNotASessionSnapshot_ReturnsFalse(string key)
    {
        // Arrange
        // Act
        bool parsed = AutoSaveService.TryParseStateKey(key, out var id);

        // Assert
        // "ab" is the case that used to throw: the old code sliced the key at the prefix length
        // without checking that the key is long enough or starts with the prefix.
        parsed.Should().BeFalse();
        id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryParseStateKey_WhenKeyIsASessionSnapshot_ReturnsTheSessionId()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        bool parsed = AutoSaveService.TryParseStateKey($"{StatePrefix}{sessionId}", out var id);

        // Assert
        parsed.Should().BeTrue();
        id.Should().Be(sessionId);
    }
}
