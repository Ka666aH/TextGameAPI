using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Services;
using TextGame.Application.Validators;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;

namespace TextGame.Tests.Application;

/// <summary>
/// Registration, login and refresh against hand-written repositories.
/// The unique-index and double-refresh tests are regression guards for notes/AGENTS.md §2.5
/// (a lost race must be a 400, not a 500) and §2.1 (the losing request of a parallel refresh
/// must not be logged out).
/// </summary>
public class AuthServiceTests
{
    private const string ValidLogin = "ka666ah";
    private const string ValidPassword = "Pass1";
    private const string Fingerprint = "browser | 127.0.0.1";

    [Fact]
    public async Task RegisterAsync_WithTooShortLogin_ThrowsValidationException()
    {
        // Arrange
        Deps deps = Create();

        // Act
        Func<Task> act = () => deps.Service.RegisterAsync("ab", ValidPassword);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        deps.Users.CreateCalls.Should().Be(0);
    }

    [Fact]
    public async Task RegisterAsync_WhenLoginIsAlreadyTaken_ThrowsValidationException()
    {
        // Arrange
        Deps deps = Create();
        deps.Users.Seed(new User(ValidLogin, deps.Hasher.Hash(ValidPassword)));

        // Act
        Func<Task> act = () => deps.Service.RegisterAsync(ValidLogin, ValidPassword);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        deps.Users.CreateCalls.Should().Be(0);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_StoresUserWithHashedPassword()
    {
        // Arrange
        Deps deps = Create();

        // Act
        await deps.Service.RegisterAsync(ValidLogin, ValidPassword);

        // Assert
        deps.Users.CreateCalls.Should().Be(1);
        deps.UnitOfWork.SaveCount.Should().Be(1);

        User? stored = await deps.Users.GetAsync(ValidLogin);
        stored.Should().NotBeNull();
        stored!.HashedPass.Should().Be(deps.Hasher.Hash(ValidPassword));
    }

    [Fact]
    public async Task RegisterAsync_WhenTheUniqueIndexFires_ThrowsValidationException()
    {
        // Arrange
        // The pre-check reads before the insert: two parallel registrations both see "free",
        // then the database rejects the second one. The repository simulates that exact state.
        Deps deps = Create();
        deps.Users.CreateException = new DbUpdateException("duplicate key value violates unique constraint \"IX_Users_Login\"");

        // Act
        Func<Task> act = () => deps.Service.RegisterAsync(ValidLogin, ValidPassword);

        // Assert
        // notes/AGENTS.md §2.5: the EF exception escapes the service, so the client gets a 500
        // instead of a 400 with "login already exists".
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LogInAsync_WhenUserDoesNotExist_ThrowsUserNotFoundException()
    {
        // Arrange
        Deps deps = Create();

        // Act
        Func<Task> act = () => deps.Service.LogInAsync(ValidLogin, ValidPassword, Fingerprint);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task LogInAsync_WhenPasswordIsWrong_ThrowsIncorrectPasswordException()
    {
        // Arrange
        Deps deps = Create();
        deps.Users.Seed(new User(ValidLogin, deps.Hasher.Hash(ValidPassword)));

        // Act
        Func<Task> act = () => deps.Service.LogInAsync(ValidLogin, "Wrong1", Fingerprint);

        // Assert
        await act.Should().ThrowAsync<IncorrectPasswordException>();
    }

    [Fact]
    public async Task LogInAsync_WithValidCredentials_IssuesBothTokens()
    {
        // Arrange
        Deps deps = Create();
        var user = new User(ValidLogin, deps.Hasher.Hash(ValidPassword));
        deps.Users.Seed(user);

        // Act
        AuthResult result = await deps.Service.LogInAsync(ValidLogin, ValidPassword, Fingerprint);

        // Assert
        result.AccessToken.Should().Be($"access-{user.Id}");
        result.RefreshToken.Should().Be("refresh-1");

        // БД хранит только SHA-256; сырой токен живёт исключительно в cookie.
        RefreshToken? stored = await deps.RefreshTokens.GetAsync(RefreshTokenHash.Of(result.RefreshToken));
        stored.Should().NotBeNull();
        stored!.Token.Should().Be(RefreshTokenHash.Of(result.RefreshToken), "the refresh token must never be stored in plain text");
        stored.HashedFingerprint.Should().Be(deps.Hasher.Hash(Fingerprint), "the fingerprint must never be stored in plain text");
        stored.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenUserDoesNotExist_ThrowsUserNotFoundException()
    {
        // Arrange
        Deps deps = Create();

        // Act
        Func<Task> act = () => deps.Service.GetCurrentAsync(Guid.NewGuid(), null);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task GetCurrentAsync_WithoutSessionClaim_ReturnsUserWithoutSession()
    {
        // Arrange
        Deps deps = Create();
        var user = new User(ValidLogin, deps.Hasher.Hash(ValidPassword));
        deps.Users.Seed(user);

        // Act
        MeResult result = await deps.Service.GetCurrentAsync(user.Id, null);

        // Assert
        // Залогинен, но игры ещё нет — фронт покажет список сессий.
        result.UserId.Should().Be(user.Id);
        result.Login.Should().Be(ValidLogin);
        result.SessionId.Should().BeNull();
        result.SessionName.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentAsync_WithOwnedSession_ReturnsThatSession()
    {
        // Arrange
        Deps deps = Create();
        var user = new User(ValidLogin, deps.Hasher.Hash(ValidPassword));
        deps.Users.Seed(user);
        var session = new Session(user.Id);
        deps.Sessions.Seed(session);

        // Act
        MeResult result = await deps.Service.GetCurrentAsync(user.Id, session.Id);

        // Assert
        result.SessionId.Should().Be(session.Id);
        result.SessionName.Should().Be(session.Name);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenSessionWasDeleted_ReturnsNoSession()
    {
        // Arrange
        Deps deps = Create();
        var user = new User(ValidLogin, deps.Hasher.Hash(ValidPassword));
        deps.Users.Seed(user);

        // Act
        MeResult result = await deps.Service.GetCurrentAsync(user.Id, Guid.NewGuid());

        // Assert
        // Протухший claim после DELETE /sessions/{id} — нормальное состояние, а не 404.
        result.SessionId.Should().BeNull();
        result.SessionName.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentAsync_WhenSessionBelongsToAnotherUser_ReturnsNoSession()
    {
        // Arrange
        Deps deps = Create();
        var user = new User(ValidLogin, deps.Hasher.Hash(ValidPassword));
        deps.Users.Seed(user);
        var foreignSession = new Session(Guid.NewGuid());
        deps.Sessions.Seed(foreignSession);

        // Act
        MeResult result = await deps.Service.GetCurrentAsync(user.Id, foreignSession.Id);

        // Assert
        result.SessionId.Should().BeNull();
        result.SessionName.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsUnknown_ThrowsRefreshTokenNotFoundException()
    {
        // Arrange
        Deps deps = Create();

        // Act
        Func<Task> act = () => deps.Service.RefreshAsync("ghost-token", "", Fingerprint);

        // Assert
        await act.Should().ThrowAsync<RefreshTokenNotFoundException>();
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsExpired_ThrowsRefreshTokenExpiredException()
    {
        // Arrange
        Deps deps = Create();
        var userId = Guid.NewGuid();
        const string rawToken = "expired-token";
        // В строке БД — хэш, в запросе — сырой токен, как это делает cookie.
        var expired = new RefreshToken(userId, RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(-1), deps.Hasher.Hash(Fingerprint));
        deps.RefreshTokens.Seed(expired);

        // Act
        Func<Task> act = () => deps.Service.RefreshAsync(rawToken, "", Fingerprint);

        // Assert
        await act.Should().ThrowAsync<RefreshTokenExpiredException>();
    }

    [Fact]
    public async Task RefreshAsync_WhenFingerprintDiffers_WipesEveryTokenOfTheUser()
    {
        // Arrange
        Deps deps = Create();
        var userId = Guid.NewGuid();
        const string rawToken = "stolen-token";
        var token = new RefreshToken(userId, RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(1), deps.Hasher.Hash("another browser | 10.0.0.1"));
        deps.RefreshTokens.Seed(token);

        // Act
        Func<Task> act = () => deps.Service.RefreshAsync(rawToken, "", Fingerprint);

        // Assert
        await act.Should().ThrowAsync<RefreshTokenCompromisedException>();
        deps.RefreshTokens.DeleteAllCalls.Should().Be(1);
        deps.UnitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_RotatesItAndIssuesAccessToken()
    {
        // Arrange
        Deps deps = Create();
        var userId = Guid.NewGuid();
        const string rawToken = "good-token";
        var token = new RefreshToken(userId, RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(1), deps.Hasher.Hash(Fingerprint));
        deps.RefreshTokens.Seed(token);

        // Act
        AuthResult result = await deps.Service.RefreshAsync(rawToken, "", Fingerprint);

        // Assert
        result.AccessToken.Should().Be($"access-{userId}");
        result.RefreshToken.Should().Be("refresh-1");
        (await deps.RefreshTokens.GetAsync(RefreshTokenHash.Of(rawToken))).Should().BeNull("the rotated token must be consumed");
    }

    [Fact]
    public async Task RefreshAsync_WhenAccessTokenCarriesForeignSession_DropsTheSession()
    {
        // Arrange
        Deps deps = Create();
        var userId = Guid.NewGuid();
        var foreignSessionId = Guid.NewGuid();
        const string rawToken = "good-token";
        var token = new RefreshToken(userId, RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(1), deps.Hasher.Hash(Fingerprint));
        deps.RefreshTokens.Seed(token);
        deps.Sessions.Seed(new Session(Guid.NewGuid()));

        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim(AccessClaims.SessionId, foreignSessionId.ToString()));
        deps.Tokens.AccessTokenPrincipal = new ClaimsPrincipal(identity);

        // Act
        await deps.Service.RefreshAsync(rawToken, "access-token", Fingerprint);

        // Assert
        // A session that belongs to someone else must not survive the refresh.
        deps.Tokens.SessionIds.Should().ContainSingle().Which.Should().BeNull();    }

    [Fact]
    public async Task RefreshAsync_WhenTheSameTokenIsUsedTwice_SecondCallDoesNotLogTheUserOut()
    {
        // Arrange
        // Two parallel requests come in with the same refresh token. The winner rotates it and
        // deletes the old row, the loser arrives a moment later and finds nothing.
        Deps deps = Create();
        const string rawToken = "racy-token";
        var token = new RefreshToken(Guid.NewGuid(), RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(1), deps.Hasher.Hash(Fingerprint));
        deps.RefreshTokens.Seed(token);
        await deps.Service.RefreshAsync(rawToken, "", Fingerprint);

        // Act
        Func<Task> secondCall = () => deps.Service.RefreshAsync(rawToken, "", Fingerprint);

        // Assert
        // notes/AGENTS.md §2.1: the loser gets RefreshTokenNotFoundException, the middleware
        // turns it into 401 + deleted cookies, and the user is logged out by a harmless double
        // submit. A grace period or single-flight is required instead.
        await secondCall.Should().NotThrowAsync();
    }

    private static Deps Create()
    {
        var users = new FakeUserRepository();
        var refreshTokens = new FakeRefreshTokenRepository();
        var tokens = new FakeTokenRepository();
        var sessions = new FakeSessionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var hasher = new FakeHasher();

        var service = new AuthService(
            new RegisterCommandValidator(),
            hasher,
            users,
            tokens,
            refreshTokens,
            sessions,
            new RefreshReplayCache(),
            unitOfWork);

        return new Deps(service, users, refreshTokens, tokens, sessions, unitOfWork, hasher);
    }

    private sealed record Deps(
        AuthService Service,
        FakeUserRepository Users,
        FakeRefreshTokenRepository RefreshTokens,
        FakeTokenRepository Tokens,
        FakeSessionRepository Sessions,
        FakeUnitOfWork UnitOfWork,
        FakeHasher Hasher);

    private sealed class FakeHasher : IHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string hashedPassword) => hashedPassword == Hash(password);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users = [];

        public Exception? CreateException { get; set; }
        public int CreateCalls { get; private set; }

        public void Seed(User user) => _users[user.Login] = user;

        public Task CreateAsync(User user, CancellationToken ct = default)
        {
            CreateCalls++;
            if (CreateException is not null) throw CreateException;

            _users[user.Login] = user;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user, CancellationToken ct = default)
        {
            _users.Remove(user.Login);

            return Task.CompletedTask;
        }

        public Task<User?> GetAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(_users.Values.FirstOrDefault(u => u.Id == userId));

        public Task<User?> GetAsync(string login, CancellationToken ct = default) =>
            Task.FromResult(_users.GetValueOrDefault(login));
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly Dictionary<string, RefreshToken> _tokens = [];

        public int DeleteAllCalls { get; private set; }

        public void Seed(RefreshToken refreshToken) => _tokens[refreshToken.Token] = refreshToken;

        public Task CreateAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            _tokens[refreshToken.Token] = refreshToken;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            _tokens.Remove(refreshToken.Token);

            return Task.CompletedTask;
        }

        public Task DeleteAllAsync(Guid userId, CancellationToken ct = default)
        {
            DeleteAllCalls++;

            foreach (string token in _tokens.Where(pair => pair.Value.UserId == userId).Select(pair => pair.Key).ToList())
            {
                _tokens.Remove(token);
            }

            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetAsync(string token, CancellationToken ct = default) =>
            Task.FromResult(_tokens.GetValueOrDefault(token));
    }

    private sealed class FakeTokenRepository : ITokenRepository
    {
        private int _refreshCounter;

        public List<Guid?> SessionIds { get; } = [];
        public ClaimsPrincipal AccessTokenPrincipal { get; set; } = new(new ClaimsIdentity("test"));

        public string GenerateAccessToken(Guid userId, Guid? gameSessionId = null)
        {
            SessionIds.Add(gameSessionId);

            return $"access-{userId}";
        }

        public (string RawToken, RefreshToken Stored) GenerateRefreshToken(Guid userId, string hashedFingerprint)
        {
            _refreshCounter++;
            string rawToken = $"refresh-{_refreshCounter}";

            // Хранится хэш, наружу — сырой токен: тот же контракт, что и в JWTRepository.
            return (rawToken, new RefreshToken(userId, RefreshTokenHash.Of(rawToken), DateTime.UtcNow.AddDays(7), hashedFingerprint));
        }

        public ClaimsPrincipal ReadTokenWithoutLifetime(string token) => AccessTokenPrincipal;
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

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;

            return Task.FromResult(1);
        }
    }
}
