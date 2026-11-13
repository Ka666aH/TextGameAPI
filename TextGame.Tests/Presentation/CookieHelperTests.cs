using Microsoft.AspNetCore.Http;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Helpers;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Auth cookie shape. The two red tests cover notes/AGENTS.md §2.9: the Secure flag is
/// hardcoded to false, so an HTTPS deployment still emits downgradable cookies.
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
    [Trait("Category", "KnownBug")]
    public void SetAccessCookie_OverHttps_MarksCookieSecure()
    {
        // Arrange
        var context = CreateContext(isHttps: true);

        // Act
        context.Response.SetAccessCookie("access-value");

        // Assert
        // notes/AGENTS.md §2.9: Secure is hardcoded false, so the browser may send this
        // cookie back over plain HTTP.
        LastSetCookie(context).Should().Contain("secure");
    }

    [Fact]
    [Trait("Category", "KnownBug")]
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
