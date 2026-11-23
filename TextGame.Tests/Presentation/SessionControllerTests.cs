using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Controllers;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Session endpoints called without a user id claim. The red tests cover
/// notes/AGENTS.md §2.7: the claim result is ignored, so the handler runs with Guid.Empty.
/// </summary>
public class SessionControllerTests
{
    [Fact]
    public async Task CreateAsync_WithUserIdClaim_CreatesSessionForThatUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId);

        // Act
        IActionResult result = await controller.CreateAsync(sessionName: "новая", seed: null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        sessionService.LastUserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsSeedAsString()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        const long seed = 9007199254740993L;
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId);

        // Act
        IActionResult result = await controller.CreateAsync(sessionName: "новая", seed: seed, CancellationToken.None);

        // Assert
        // notes/backend-requests.md п. 15: то же самое, что и в SessionDTO, — иначе клиент
        // получает number в POST /sessions и string в GET /sessions.
        var created = result.Should().BeOfType<CreatedResult>().Which;
        created.Value.Should().BeEquivalentTo(new
        {
            sessionId = sessionService.LastSession!.Id,
            sessionName = "новая",
            seed = "9007199254740993"
        });
    }

    [Fact]
    public async Task CreateAsync_WithoutUserIdClaim_ThrowsMissingUserIdClaimException()
    {
        // Arrange
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId: null);

        // Act
        Func<Task> act = () => controller.CreateAsync(sessionName: "новая", seed: null, CancellationToken.None);

        // Assert
        // notes/AGENTS.md §2.7: TryGetUserId's result is discarded, so the session is
        // created for Guid.Empty instead of rejecting the request.
        await act.Should().ThrowAsync<MissingUserIdClaimException>();
        sessionService.LastUserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetSessionsAsync_WithoutUserIdClaim_ThrowsMissingUserIdClaimException()
    {
        // Arrange
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId: null);

        // Act
        Func<Task> act = () => controller.GetSessionsAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<MissingUserIdClaimException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenActiveSessionIsDeleted_ClearsTheSessionClaimInANewAccessCookie()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId, sessionId);

        // Act
        IActionResult result = await controller.DeleteAsync(sessionId, CancellationToken.None);

        // Assert
        // В куке не должно остаться claim на удалённую игру, иначе следующий /state даёт 404.
        result.Should().BeOfType<NoContentResult>();
        sessionService.DeletedSessionId.Should().Be(sessionId);
        sessionService.CreateAccessTokenCalls.Should().Be(1);
        controller.HttpContext.Response.Headers.SetCookie.ToString().Should().Contain("AccessToken=");
    }

    [Fact]
    public async Task DeleteAsync_WhenAnotherSessionIsDeleted_KeepsTheCurrentGame()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid activeSessionId = Guid.NewGuid();
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId, activeSessionId);

        // Act
        IActionResult result = await controller.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        sessionService.CreateAccessTokenCalls.Should().Be(0);
        controller.HttpContext.Response.Headers.SetCookie.ToString().Should().BeEmpty();
    }

    private static SessionController CreateController(RecordingSessionService sessionService, Guid? userId, Guid? sessionId = null)
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity("test");
        if (userId.HasValue)
        {
            identity.AddClaim(new Claim(AccessClaims.UserId, userId.Value.ToString()));
        }
        if (sessionId.HasValue)
        {
            identity.AddClaim(new Claim(AccessClaims.SessionId, sessionId.Value.ToString()));
        }

        httpContext.User = new ClaimsPrincipal(identity);

        return new SessionController(sessionService)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };
    }

    private sealed class RecordingSessionService : ISessionService
    {
        public Guid? LastUserId { get; private set; }
        public Guid? DeletedSessionId { get; private set; }
        public Session? LastSession { get; private set; }
        public int CreateAccessTokenCalls { get; private set; }

        public Task<Session> CreateAsync(Guid userId, string? sessionName, long? seed = null, CancellationToken ct = default)
        {
            LastUserId = userId;
            LastSession = new Session(userId, sessionName, seed);

            return Task.FromResult(LastSession!);
        }

        public Task<string> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct = default) =>
            Task.FromResult("access-token");

        public Task DeleteAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
        {
            LastUserId = userId;
            DeletedSessionId = sessionId;

            return Task.CompletedTask;
        }

        public string CreateAccessToken(Guid userId)
        {
            CreateAccessTokenCalls++;

            return "access-token";
        }

        public Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default)
        {
            LastUserId = userId;

            return Task.FromResult<IReadOnlyList<Session>>([]);
        }
    }
}
