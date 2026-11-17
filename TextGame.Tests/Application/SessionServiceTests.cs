using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;

namespace TextGame.Tests.Application;

/// <summary>
/// Session loading. Both red tests cover notes/AGENTS.md §1.2: LoadAsync hands out an access
/// token without checking that the session exists or belongs to the caller.
/// </summary>
public class SessionServiceTests
{
    [Fact]
    public async Task LoadAsync_WhenSessionExistsAndBelongsToTheUser_ReturnsAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = new FakeSessionRepository();
        var session = new Session(userId);
        sessions.Seed(session);
        var service = CreateService(sessions);

        // Act
        string accessToken = await service.LoadAsync(userId, session.Id);

        // Assert
        accessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task LoadAsync_WhenSessionDoesNotExist_ThrowsSessionNotFoundException()
    {
        // Arrange
        var sessions = new FakeSessionRepository();
        var service = CreateService(sessions);

        // Act
        Func<Task> act = () => service.LoadAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        // notes/AGENTS.md §1.2: the method never touches the repository, so a random guid in the
        // route produces an access token for a session that is not in the database.
        await act.Should().ThrowAsync<SessionNotFoundException>();
    }

    [Fact]
    public async Task LoadAsync_WhenSessionBelongsToAnotherUser_ThrowsNotSessionOwnerException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var sessions = new FakeSessionRepository();
        var session = new Session(ownerId);
        sessions.Seed(session);
        var service = CreateService(sessions);

        // Act
        Func<Task> act = () => service.LoadAsync(attackerId, session.Id);

        // Assert
        await act.Should().ThrowAsync<NotSessionOwnerException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenSessionBelongsToAnotherUser_ThrowsNotSessionOwnerException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var sessions = new FakeSessionRepository();
        var session = new Session(ownerId);
        sessions.Seed(session);
        var service = CreateService(sessions);

        // Act
        Func<Task> act = () => service.DeleteAsync(attackerId, session.Id);

        // Assert
        await act.Should().ThrowAsync<NotSessionOwnerException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenSessionDoesNotExist_ThrowsSessionNotFoundException()
    {
        // Arrange
        var sessions = new FakeSessionRepository();
        var service = CreateService(sessions);

        // Act
        Func<Task> act = () => service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<SessionNotFoundException>();
    }

    private static SessionService CreateService(ISessionRepository sessions) =>
        // Only LoadAsync is under test here: the save/cache/factory collaborators are never
        // reached, so they stay null on purpose.
        new(sessions, null!, null!, null!, null!, new FakeTokenRepository());

    private sealed class FakeTokenRepository : ITokenRepository
    {
        public string GenerateAccessToken(Guid userId, Guid? gameSessionId = null) => "access-token";

        public (string RawToken, RefreshToken Stored) GenerateRefreshToken(Guid userId, string hashedFingerprint)
        {
            string rawToken = $"refresh-{Guid.NewGuid():N}";

            return (rawToken, new RefreshToken(userId, RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(7), hashedFingerprint));
        }

        public System.Security.Claims.ClaimsPrincipal ReadTokenWithoutLifetime(string token) =>
            new(new System.Security.Claims.ClaimsIdentity("test"));
    }

    private sealed class FakeSessionRepository : ISessionRepository
    {
        private readonly Dictionary<Guid, Session> _sessions = [];

        public void Seed(Session session) => _sessions[session.Id] = session;

        public Task CreateAsync(Session session, CancellationToken ct = default)
        {
            _sessions[session.Id] = session;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Session session, CancellationToken ct = default)
        {
            _sessions.Remove(session.Id);

            return Task.CompletedTask;
        }

        public Task<Session?> GetAsync(Guid sessionId, CancellationToken ct = default) =>
            Task.FromResult(_sessions.GetValueOrDefault(sessionId));

        public Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Session>>([.. _sessions.Values.Where(session => session.UserId == userId)]);
    }
}
