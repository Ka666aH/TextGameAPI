using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Application.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Tests.Application;

/// <summary>
/// Saving. The manual-save tests cover notes/backend-requests.md п. 8: the state comes from
/// <see cref="ISessionProvider"/>, so an evicted Redis snapshot falls back to the last save
/// instead of failing with SESSION_NOT_FOUND.
/// </summary>
public class SaveServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenManualAndCacheIsEmpty_ReadsStateFromSessionProvider()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var providerState = new State { Coins = 7 };
        var provider = new FakeSessionProvider(providerState);
        var cache = new EmptyStateCache();
        var saves = new RecordingSaveRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(saves, unitOfWork, cache, provider);

        // Act
        Guid saveId = await service.CreateAsync(sessionId, SaveType.Manual, "ручное");

        // Assert
        var save = saves.Created.Should().ContainSingle().Which;
        save.Id.Should().Be(saveId);
        save.Type.Should().Be(SaveType.Manual);
        save.State.Should().BeSameAs(providerState);
        provider.Calls.Should().Be(1);
        cache.GetCalls.Should().Be(0, "ручное сохранение больше не читает кеш напрямую");
        unitOfWork.Saves.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WhenManualAndNeitherCacheNorSavesHaveState_ThrowsSaveNotFoundException()
    {
        // Arrange
        var service = CreateService(
            new RecordingSaveRepository(),
            new RecordingUnitOfWork(),
            new EmptyStateCache(),
            new FakeSessionProvider(state: null));

        // Act
        Func<Task> act = () => service.CreateAsync(Guid.NewGuid(), SaveType.Manual, "ручное");

        // Assert
        // Провайдер бросает SAVE_NOT_FOUND, а не SESSION_NOT_FOUND: это и есть ожидаемый
        // побочный эффект исправления (notes/fixes-1.2.0.md, п. 7).
        await act.Should().ThrowAsync<SaveNotFoundException>();
    }

    [Fact]
    public async Task SaveAutoAsync_WhenStateIsCached_ReplacesTheAutoSave()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var state = new State { Coins = 3 };
        var cache = new StubStateCache(state);
        var saves = new RecordingSaveRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(saves, unitOfWork, cache, new FakeSessionProvider(state));

        // Act
        await service.SaveAutoAsync(sessionId);

        // Assert
        // notes/fixes-1.2.0.md, п. 11: именно замена, а не новая строка — иначе автосейвов
        // на сессию становится больше одного.
        saves.ReplacedSessionIds.Should().ContainSingle().Which.Should().Be(sessionId);
        saves.ReplacedSaves.Should().ContainSingle().Which.Type.Should().Be(SaveType.Auto);
        saves.Created.Should().BeEmpty();
        unitOfWork.Saves.Should().Be(1);
    }

    [Fact]
    public async Task SaveAutoAsync_WhenStateIsNotCached_DoesNothing()
    {
        // Arrange
        var saves = new RecordingSaveRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(saves, unitOfWork, new EmptyStateCache(), new FakeSessionProvider(state: null));

        // Act
        await service.SaveAutoAsync(Guid.NewGuid());

        // Assert
        saves.ReplacedSaves.Should().BeEmpty();
        unitOfWork.Saves.Should().Be(0);
    }

    private static SaveService CreateService(
        ISaveRepository saves,
        IUnitOfWork unitOfWork,
        IStateCacheService cache,
        ISessionProvider provider) =>
        new(saves, unitOfWork, cache, new FakeSaveFactory(), provider);

    private sealed class FakeSessionProvider : ISessionProvider
    {
        private readonly State? _state;

        public FakeSessionProvider(State? state) => _state = state;

        public int Calls { get; private set; }

        public Task<State> GetAsync(Guid sessionId, CancellationToken ct = default)
        {
            Calls++;
            if (_state is null) throw new SaveNotFoundException();

            return Task.FromResult(_state!);
        }
    }

    private sealed class EmptyStateCache : IStateCacheService
    {
        public int GetCalls { get; private set; }

        public Task SetAsync(Guid sessionId, State state, CancellationToken ct = default) =>
            throw new InvalidOperationException("Кеш не должен использоваться в этих сценариях.");

        public Task<State?> GetAsync(Guid sessionId, CancellationToken ct = default)
        {
            GetCalls++;
            return Task.FromResult<State?>(null);
        }

        public Task DeleteAsync(Guid sessionId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubStateCache : IStateCacheService
    {
        private readonly State? _state;

        public StubStateCache(State? state) => _state = state;

        public Task SetAsync(Guid sessionId, State state, CancellationToken ct = default) => Task.CompletedTask;

        public Task<State?> GetAsync(Guid sessionId, CancellationToken ct = default) => Task.FromResult(_state);

        public Task DeleteAsync(Guid sessionId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeSaveFactory : ISaveFactory
    {
        public Save CreateInitial(Guid sessionId, long seed) =>
            new(sessionId, SaveType.Initial, "начальное", new State());

        public Save CreateManual(Guid sessionId, string? name, State state) =>
            new(sessionId, SaveType.Manual, name ?? "ручное", state);

        public Save CreateAuto(Guid sessionId, State state) =>
            new(sessionId, SaveType.Auto, "авто", state);
    }

    private sealed class RecordingSaveRepository : ISaveRepository
    {
        public List<Save> Created { get; } = [];
        public List<Guid> ReplacedSessionIds { get; } = [];
        public List<Save> ReplacedSaves { get; } = [];

        public Task CreateAsync(Save save, CancellationToken ct = default)
        {
            Created.Add(save);
            return Task.CompletedTask;
        }

        public Task<Save?> GetAsyncWithTrack(Guid saveId, CancellationToken ct = default) =>
            Task.FromResult<Save?>(null);

        public Task<Save?> GetAsync(Guid saveId, CancellationToken ct = default) =>
            Task.FromResult<Save?>(null);

        public Task<Save?> GetLastSaveAsync(Guid sessionId, CancellationToken ct = default) =>
            Task.FromResult<Save?>(null);

        public Task<IReadOnlyList<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Save>>([]);

        public Task DeleteAsync(Save save, CancellationToken ct = default) => Task.CompletedTask;

        public Task<Dictionary<Guid, string?>> GetAutoSaveHashesAsync(IReadOnlyList<Guid> sessionIds, CancellationToken ct) =>
            Task.FromResult(new Dictionary<Guid, string?>());

        public Task BatchReplaceAutoSavesAsync(IReadOnlyList<Guid> sessionIds, IReadOnlyList<Save> saves, CancellationToken ct)
        {
            ReplacedSessionIds.AddRange(sessionIds);
            ReplacedSaves.AddRange(saves);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int Saves { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            Saves++;
            return Task.FromResult(1);
        }
    }
}
