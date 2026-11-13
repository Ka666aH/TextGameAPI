using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Attributes;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Session ownership filter. Both red tests cover notes/AGENTS.md §1.2 (IDOR): the filter
/// trusts the SessionId claim instead of the session id from the route and stays silent
/// when the claim is missing.
/// </summary>
public class RequireSessionOwnershipAttributeTests
{
    [Fact]
    public async Task OnAuthorizationAsync_WhenClaimMatchesRoute_ChecksThatSession()
    {
        // Arrange
        Guid sessionId = Guid.NewGuid();
        var (context, guard) = CreateContext(claimSessionId: sessionId, routeSessionId: sessionId);
        var attribute = new RequireSessionOwnershipAttribute();

        // Act
        await attribute.OnAuthorizationAsync(context);

        // Assert
        guard.CheckedSessionIds.Should().ContainSingle().Which.Should().Be(sessionId);
    }

    [Fact]
    [Trait("Category", "KnownBug")]
    public async Task OnAuthorizationAsync_WhenClaimDiffersFromRoute_ChecksTheSessionFromTheRoute()
    {
        // Arrange
        Guid attackerSessionId = Guid.NewGuid();
        Guid victimSessionId = Guid.NewGuid();
        var (context, guard) = CreateContext(claimSessionId: attackerSessionId, routeSessionId: victimSessionId);
        var attribute = new RequireSessionOwnershipAttribute();

        // Act
        await attribute.OnAuthorizationAsync(context);

        // Assert
        // notes/AGENTS.md §1.2: DELETE /sessions/{id} currently validates the caller's own
        // session, so the id in the route is never checked against the owner.
        guard.CheckedSessionIds.Should().ContainSingle().Which.Should().Be(victimSessionId);
    }

    [Fact]
    [Trait("Category", "KnownBug")]
    public async Task OnAuthorizationAsync_WhenSessionIdClaimIsMissing_ThrowsMissingSessionIdClaimException()
    {
        // Arrange
        var (context, _) = CreateContext(claimSessionId: null, routeSessionId: Guid.NewGuid());
        var attribute = new RequireSessionOwnershipAttribute();

        // Act
        Func<Task> act = () => attribute.OnAuthorizationAsync(context);

        // Assert
        // notes/AGENTS.md §1.2: no claim means the filter skips the check silently, which
        // turns it into a no-op instead of a 401.
        await act.Should().ThrowAsync<MissingSessionIdClaimException>();
    }

    private static (AuthorizationFilterContext Context, RecordingSessionAccessGuard Guard) CreateContext(
        Guid? claimSessionId,
        Guid? routeSessionId)
    {
        var guard = new RecordingSessionAccessGuard();
        var services = new ServiceCollection();
        services.AddSingleton<ISessionAccessGuard>(guard);

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        var identity = new ClaimsIdentity("test");
        if (claimSessionId.HasValue)
        {
            identity.AddClaim(new Claim(AccessClaims.SessionId, claimSessionId.Value.ToString()));
        }

        httpContext.User = new ClaimsPrincipal(identity);

        var routeData = new RouteData();
        if (routeSessionId.HasValue)
        {
            routeData.Values["sessionId"] = routeSessionId.Value.ToString();
        }

        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());

        return (new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>()), guard);
    }

    private sealed class RecordingSessionAccessGuard : ISessionAccessGuard
    {
        public List<Guid> CheckedSessionIds { get; } = [];

        public Task EnsureOwnershipAsync(Guid sessionId, CancellationToken ct)
        {
            CheckedSessionIds.Add(sessionId);

            return Task.CompletedTask;
        }
    }
}
