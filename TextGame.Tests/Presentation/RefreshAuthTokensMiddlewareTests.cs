using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Services;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Middleware;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Token rotation middleware. The red test covers notes/AGENTS.md §1.3: a request with no
/// cookies at all dies inside the middleware, so anonymous endpoints (Swagger, health)
/// can never reach their own pipeline.
/// </summary>
public class RefreshAuthTokensMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenAccessTokenIsStillValid_PassesThroughWithoutRefresh()
    {
        // Arrange
        var authService = new FakeAuthService();
        var context = CreateContext(authService);
        context.Request.Headers.Cookie = $"{TokenParameters.AccessToken}={CreateValidAccessToken()}";
        bool nextCalled = false;
        var middleware = new RefreshAuthTokens(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        authService.RefreshCalls.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_WhenOnlyRefreshTokenExists_RotatesTokensAndPassesThrough()
    {
        // Arrange
        var authService = new FakeAuthService { RefreshResult = new AuthResult("new-refresh", "new-access") };
        var context = CreateContext(authService);
        context.Request.Headers.Cookie = $"{TokenParameters.RefreshToken}=old-refresh";
        bool nextCalled = false;
        var middleware = new RefreshAuthTokens(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        authService.RefreshCalls.Should().Be(1);
        authService.LastRefreshToken.Should().Be("old-refresh");
        context.Response.Headers.SetCookie.ToString().Should().Contain("new-access");
    }

    [Fact]
    public async Task InvokeAsync_WithoutAnyTokens_PassesThroughToTheNextMiddleware()
    {
        // Arrange
        var authService = new FakeAuthService();
        var context = CreateContext(authService);
        bool nextCalled = false;
        var middleware = new RefreshAuthTokens(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        Func<Task> act = () => middleware.InvokeAsync(context);

        // Assert
        // notes/AGENTS.md §1.3: no cookies must mean "not logged in", not "explode here".
        // The 401 has to come from [Authorize] further down the pipeline.
        await act.Should().NotThrowAsync();
        nextCalled.Should().BeTrue();
    }

    private static DefaultHttpContext CreateContext(IAuthService authService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authService);

        return new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
    }

    private static string CreateValidAccessToken()
    {
        var token = new JwtSecurityToken(expires: DateTime.UtcNow.AddHours(1));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class FakeAuthService : IAuthService
    {
        public int RefreshCalls { get; private set; }
        public string? LastRefreshToken { get; private set; }
        public AuthResult? RefreshResult { get; init; }

        public Task RegisterAsync(string login, string password, CancellationToken ct = default) => Task.CompletedTask;

        public Task<AuthResult> LogInAsync(string login, string password, string fingerprint, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<AuthResult> RefreshAsync(string refreshToken, string accessToken, string fingerprint, CancellationToken ct = default)
        {
            RefreshCalls++;
            LastRefreshToken = refreshToken;

            return Task.FromResult(RefreshResult!);
        }

        public Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default) => Task.CompletedTask;
    }
}
