using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Cache;
using TextGame.Infrastructure.Database;

namespace TextGame.Tests.Integration;

/// <summary>
/// The real Postgres and Redis behind the API, borrowed from its own service provider.
/// The schema test proves the unique login index exists (§2.5), the cache tests prove a corrupt
/// snapshot surfaces as a game exception instead of an empty State (§2.6).
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class StorageTests : IntegrationTestBase
{
    public StorageTests(GameApiFactory factory) : base(factory) { }

    [Fact]
    public async Task UsersTable_RejectsDuplicateLogin()
    {
        // Arrange
        string login = $"dup_{Guid.NewGuid():N}";
        using (IServiceScope firstScope = Factory.Services.CreateScope())
        {
            AppDbContext db = firstScope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.Add(new User(login, "hash"));
            await db.SaveChangesAsync(Ct);
        }

        // Act
        using IServiceScope secondScope = Factory.Services.CreateScope();
        AppDbContext secondDb = secondScope.ServiceProvider.GetRequiredService<AppDbContext>();
        secondDb.Users.Add(new User(login, "hash"));
        Func<Task> act = () => secondDb.SaveChangesAsync(Ct);

        // Assert
        // notes/AGENTS.md §2.5: this is the constraint that a parallel registration hits.
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task StateCache_RoundTripsStateThroughRedis()
    {
        // Arrange
        IStateCacheService cache = Factory.Services.GetRequiredService<IStateCacheService>();
        Guid sessionId = Guid.NewGuid();

        // Act
        await cache.SetAsync(sessionId, new State { Coins = 42, Keys = 3 }, Ct);
        State? restored = await cache.GetAsync(sessionId, Ct);
        await cache.DeleteAsync(sessionId, Ct);

        // Assert
        restored.Should().NotBeNull();
        restored!.Coins.Should().Be(42);
        restored.Keys.Should().Be(3);
    }

    [Fact]
    public async Task StateCache_WhenPayloadIsCorrupt_ThrowsGameException()
    {
        // Arrange
        IStateCacheService cache = Factory.Services.GetRequiredService<IStateCacheService>();
        ICacheRepository repository = Factory.Services.GetRequiredService<ICacheRepository>();
        Guid sessionId = Guid.NewGuid();
        await repository.SetAsync($"{CacheParameters.StateKeyPrefix}{sessionId}", "{ broken", TimeSpan.FromMinutes(1), Ct);

        // Act
        Func<Task> act = () => cache.GetAsync(sessionId, Ct);

        // Assert
        // notes/AGENTS.md §2.6: a corrupt snapshot must surface as a game exception.
        await act.Should().ThrowAsync<GameException>();

        await cache.DeleteAsync(sessionId, Ct);
    }

    [Fact]
    public async Task StateCache_WhenPayloadIsJsonNull_ThrowsGameException()
    {
        // Arrange
        IStateCacheService cache = Factory.Services.GetRequiredService<IStateCacheService>();
        ICacheRepository repository = Factory.Services.GetRequiredService<ICacheRepository>();
        Guid sessionId = Guid.NewGuid();
        await repository.SetAsync($"{CacheParameters.StateKeyPrefix}{sessionId}", "null", TimeSpan.FromMinutes(1), Ct);

        // Act
        Func<Task> act = () => cache.GetAsync(sessionId, Ct);

        // Assert
        // notes/AGENTS.md §2.6: "null" used to become an empty State, so the player loaded an
        // empty map instead of seeing an error.
        await act.Should().ThrowAsync<GameException>();

        await cache.DeleteAsync(sessionId, Ct);
    }

    [Fact]
    public async Task CacheRepository_KeyWalkFindsEveryStateSnapshotAndNothingElse()
    {
        // Arrange
        ICacheRepository repository = Factory.Services.GetRequiredService<ICacheRepository>();
        Guid[] sessionIds = [.. Enumerable.Range(0, 25).Select(_ => Guid.NewGuid())];
        string noiseKey = $"noise:{Guid.NewGuid():N}";

        foreach (Guid sessionId in sessionIds)
            await repository.SetAsync($"{CacheParameters.StateKeyPrefix}{sessionId}", "{}", TimeSpan.FromMinutes(2), Ct);
        await repository.SetAsync(noiseKey, "{}", TimeSpan.FromMinutes(2), Ct);

        // Act
        IReadOnlyList<string> keys = await repository.GetKeysAsync($"{CacheParameters.StateKeyPrefix}*", Ct);

        // Assert
        // notes/AGENTS.md §2.4: the sweep walks keys with SCAN instead of the blocking KEYS *.
        // It must still see every snapshot and must not pick up keys outside the State: namespace.
        keys.Should().Contain(sessionIds.Select(id => $"{CacheParameters.StateKeyPrefix}{id}"));
        keys.Should().NotContain(noiseKey);
        keys.Should().NotContainNulls();

        foreach (Guid sessionId in sessionIds)
            await repository.DeleteAsync($"{CacheParameters.StateKeyPrefix}{sessionId}", Ct);
        await repository.DeleteAsync(noiseKey, Ct);
    }
}
