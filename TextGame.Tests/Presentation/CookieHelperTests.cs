using Microsoft.AspNetCore.Http;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Helpers;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Auth cookie shape. The two HTTPS tests are regression guards for notes/AGENTS.md §2.9:
/// the Secure flag must follow the request scheme so an HTTPS deployment never emits a
/// cookie that a browser will happily send back over plain HTTP.
/// </summary>
public class CookieHelperTests
{
    [Fact]
    public void SetAccessCookie_AlwaysMarksCookieHttpOnly()
    {
        // Arrange
        var context = CreateContext(isHttps: false);

        // Act
        context.Response.SetAccessCookie("access-value");

        // Assert
        string header = LastSetCookie(context);
        header.Should().Contain($"{TokenParameters.AccessToken.ToLowerInvariant()}=access-value");
        header.Should().Contain("httponly");
        header.Should().Contain("path=/");
        header.Should().Contain("samesite=lax");
        header.Should().Contain("expires=");
    }

    [Fact]
    public void SetRefreshCookie_AlwaysMarksCookieHttpOnly()
    {
        // Arrange
        var context = CreateContext(isHttps: false);

        // Act
        context.Response.SetRefreshCookie("refresh-value");

        // Assert
        string header = LastSetCookie(context);
        header.Should().Contain($"{TokenParameters.RefreshToken.ToLowerInvariant()}=refresh-value");
        header.Should().Contain("httponly");
    }

    [Fact]
    public void SetAccessCookie_OverHttps_MarksCookieSecure()
    {
        // Arrange
        var context = CreateContext(isHttps: true);

        // Act
        context.Response.SetAccessCookie("access-value");

        // Assert
        // notes/AGENTS.md §2.9: Secure used to be hardcoded false, so the browser would
        // send this cookie back over plain HTTP.
        LastSetCookie(context).Should().Contain("secure");
    }

    [Fact]
    public void SetRefreshCookie_OverHttps_MarksCookieSecure()
    {
        // Arrange
        var context = CreateContext(isHttps: true);

        // Act
        context.Response.SetRefreshCookie("refresh-value");

        // Assert
        LastSetCookie(context).Should().Contain("secure");
    }

    [Fact]
    public void TryGetRefreshToken_WhenCookieIsMissing_ReturnsFalse()
    {
        // Arrange
        var context = CreateContext(isHttps: false);

        // Act
        bool found = context.Request.TryGetRefreshToken(out string refreshToken);

        // Assert
        found.Should().BeFalse();
        refreshToken.Should().BeEmpty();
    }

    [Fact]
    public void TryGetAccessToken_WhenCookieIsPresent_ReturnsValue()
    {
        // Arrange
        var context = CreateContext(isHttps: false);
        context.Request.Headers.Cookie = $"{TokenParameters.AccessToken}=access-value";

        // Act
        bool found = context.Request.TryGetAccessToken(out string accessToken);

        // Assert
        found.Should().BeTrue();
        accessToken.Should().Be("access-value");
    }

    [Fact]
    public void GetRequiredRefreshToken_WhenCookieIsPresent_ReturnsValue()
    {
        // Arrange
        var context = CreateContext(isHttps: false);
        context.Request.Headers.Cookie = $"{TokenParameters.RefreshToken}=refresh-value";

        // Act
        string refreshToken = context.Request.GetRequiredRefreshToken();

        // Assert
        refreshToken.Should().Be("refresh-value");
    }

    [Fact]
    public void GetRequiredRefreshToken_WhenCookieIsMissing_Throws()
    {
        // Arrange
        var context = CreateContext(isHttps: false);

        // Act
        Action act = () => context.Request.GetRequiredRefreshToken();

        // Assert
        // The same failure the logout endpoint used to raise on its own before the helper existed.
        act.Should().Throw<RefreshTokenNotFoundException>();
    }

    [Fact]
    public void DeleteAuthCookies_ExpiresBothAuthCookies()
    {
        // Arrange
        var context = CreateContext(isHttps: false);

        // Act
        context.Response.DeleteAuthCookies();

        // Assert
        string headers = context.Response.Headers.SetCookie.ToString()!.ToLowerInvariant();
        headers.Should().Contain(TokenParameters.AccessToken.ToLowerInvariant());
        headers.Should().Contain(TokenParameters.RefreshToken.ToLowerInvariant());
        headers.Should().Contain("expires=thu, 01 jan 1970");
    }

    private static DefaultHttpContext CreateContext(bool isHttps)
    {
        var context = new DefaultHttpContext();
        context.Request.IsHttps = isHttps;

        return context;
    }

    private static string LastSetCookie(DefaultHttpContext context) =>
        context.Response.Headers.SetCookie.ToString()!.ToLowerInvariant();
}
