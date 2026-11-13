using System.Net;
using System.Net.Http.Json;
using TextGame.Presentation.DTO;

namespace TextGame.Tests.Integration;

/// <summary>
/// Auth endpoints over real HTTP: cookies, status codes and the validation contract that the
/// frontend will consume.
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(GameApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidCredentials_SetsBothAuthCookies()
    {
        // Arrange
        HttpClient client = CreateClient();
        AuthRequest credentials = NewCredentials();

        // Act
        HttpResponseMessage response = await RegisterAsync(client, credentials);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        IEnumerable<string> setCookies = response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
            ? values
            : [];
        string cookies = string.Join(";", setCookies);
        cookies.Should().Contain("AccessToken=");
        cookies.Should().Contain("RefreshToken=");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = CreateClient();
        AuthRequest credentials = NewCredentials();
        (await RegisterAsync(client, credentials)).EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/auth/login",
            credentials with { Password = "Wrong1" },
            Ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithDuplicateLogin_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = CreateClient();
        AuthRequest credentials = NewCredentials();
        (await RegisterAsync(client, credentials)).EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage response = await RegisterAsync(client, credentials);

        // Assert
        // notes/AGENTS.md §2.5: the sequential duplicate is caught, the parallel one is not.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSessions_WithoutCookies_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/sessions", Ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Health_WithoutCookies_ReturnsOk()
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health", Ct);

        // Assert
        // /health carries BypassRefresh, this is the pattern the anonymous endpoints need.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
