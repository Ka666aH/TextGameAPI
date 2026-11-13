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
        IActionResult result = await controller.CreateAsync(sessionName: "новая", CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        sessionService.LastUserId.Should().Be(userId);
    }

    [Fact]
    [Trait("Category", "KnownBug")]
    public async Task CreateAsync_WithoutUserIdClaim_ThrowsMissingUserIdClaimException()
    {
        // Arrange
        var sessionService = new RecordingSessionService();
        var controller = CreateController(sessionService, userId: null);

        // Act
        Func<Task> act = () => controller.CreateAsync(sessionName: "новая", CancellationToken.None);

        // Assert
        // notes/AGENTS.md §2.7: TryGetUserId's result is discarded, so the session is
        // created for Guid.Empty instead of rejecting the request.
        await act.Should().ThrowAsync<MissingUserIdClaimException>();
        sessionService.LastUserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "KnownBug")]
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

    private static SessionController CreateController(RecordingSessionService sessionService, Guid? userId)
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity("test");
        if (userId.HasValue)
        {
            identity.AddClaim(new Claim(AccessClaims.UserId, userId.Value.ToString()));
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

        public Task<Guid> CreateAsync(Guid userId, string? sessionName, CancellationToken ct = default)
        {
            LastUserId = userId;

            return Task.FromResult(Guid.NewGuid());
        }

        public Task<string> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct = default) =>
            Task.FromResult("access-token");

        public Task DeleteAsync(Guid sessionId, CancellationToken ct = default) => Task.CompletedTask;

        public Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default)
        {
            LastUserId = userId;

            return Task.FromResult<IReadOnlyList<Session>>([]);
        }
    }
}
