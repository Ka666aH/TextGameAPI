using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TextGame.Domain.GameText;
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
        // POST /users создаёт аккаунт и сразу логинит: 201 + обе куки.
        response.StatusCode.Should().Be(HttpStatusCode.Created);
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
            "/auth/tokens",
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
    public async Task GetMe_WithoutCookies_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/auth/me", Ct);

        // Assert
        // Bootstrap-эндпоинт: без кук фронт показывает экран логина.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_AfterRegister_ReturnsLoginWithoutActiveSession()
    {
        // Arrange
        HttpClient client = CreateClient();
        AuthRequest credentials = NewCredentials();
        (await RegisterAsync(client, credentials)).EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage response = await client.GetAsync("/auth/me", Ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        body.GetProperty("userId").GetGuid().Should().NotBeEmpty();
        body.GetProperty("login").GetString().Should().Be(credentials.Login);
        body.GetProperty("sessionId").ValueKind.Should().Be(JsonValueKind.Null,
            "регистрация входит в аккаунт, но игру ещё не выбирает");
        body.GetProperty("sessionName").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetMe_AfterSessionIsCreated_ReturnsThatSession()
    {
        // Arrange
        HttpClient client = CreateClient();
        (await RegisterAsync(client, NewCredentials())).EnsureSuccessStatusCode();
        Guid sessionId = await CreateSessionAsync(client);

        // Act
        HttpResponseMessage response = await client.GetAsync("/auth/me", Ct);

        // Assert
        response.EnsureSuccessStatusCode();
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        body.GetProperty("sessionId").GetGuid().Should().Be(sessionId);
        body.GetProperty("sessionName").GetString().Should().Be(GeneralLabels.SessionDefaultName);
    }

    [Fact]
    public async Task GetMe_AfterActiveSessionIsDeleted_ReturnsNoSession()
    {
        // Arrange
        HttpClient client = CreateClient();
        (await RegisterAsync(client, NewCredentials())).EnsureSuccessStatusCode();
        Guid sessionId = await CreateSessionAsync(client);
        (await client.DeleteAsync($"/sessions/{sessionId}", Ct)).EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage response = await client.GetAsync("/auth/me", Ct);

        // Assert
        // Удаление активной игры снимает claim из access-куки: следующие /state не уйдут в 404.
        response.EnsureSuccessStatusCode();
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        body.GetProperty("sessionId").ValueKind.Should().Be(JsonValueKind.Null);
        body.GetProperty("sessionName").ValueKind.Should().Be(JsonValueKind.Null);
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
