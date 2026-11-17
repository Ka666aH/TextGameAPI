using System.Net;
using System.Net.Http.Json;
using TextGame.Presentation.DTO;

namespace TextGame.Tests.Integration;

/// <summary>
/// Two real users over real HTTP. Both red tests are the IDOR from notes/AGENTS.md §1.2: the
/// ownership filter validates the session stored in the caller's own claim, so the session id
/// from the request is never checked against the owner.
/// </summary>
[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class SessionOwnershipTests : IntegrationTestBase
{
    public SessionOwnershipTests(GameApiFactory factory) : base(factory) { }

    [Fact]
    public async Task LoadSession_OfTheSameUser_ReturnsOk()
    {
        // Arrange
        HttpClient client = CreateClient();
        (await RegisterAsync(client, NewCredentials())).EnsureSuccessStatusCode();
        Guid sessionId = await CreateSessionAsync(client);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync("/sessions/current", new { id = sessionId }, Ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoadSession_OfAnotherUser_ReturnsForbidden()
    {
        // Arrange
        HttpClient victim = CreateClient();
        (await RegisterAsync(victim, NewCredentials())).EnsureSuccessStatusCode();
        Guid victimSessionId = await CreateSessionAsync(victim);

        HttpClient attacker = CreateClient();
        (await RegisterAsync(attacker, NewCredentials())).EnsureSuccessStatusCode();
        await CreateSessionAsync(attacker);

        // Act
        HttpResponseMessage response = await attacker.PutAsJsonAsync("/sessions/current", new { id = victimSessionId }, Ct);

        // Assert
        // notes/AGENTS.md §1.2: the attacker gets an access token for a session they do not own.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LoadSession_WithoutActiveSession_ReturnsOk()
    {
        // Arrange
        HttpClient client = CreateClient();
        AuthRequest credentials = NewCredentials();
        (await RegisterAsync(client, credentials)).EnsureSuccessStatusCode();
        Guid sessionId = await CreateSessionAsync(client);
        // Повторный вход выдаёт access-токен без claim SessionId — «залогинен, игры нет».
        (await client.PostAsJsonAsync("/auth/tokens", credentials, Ct)).EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync("/sessions/current", new { id = sessionId }, Ct);

        // Assert
        // Выбор игры не должен требовать уже выбранной игры: это и есть bootstrap после логина.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteSession_WithoutActiveSession_ReturnsNoContent()
    {
        // Arrange
        HttpClient client = CreateClient();
        AuthRequest credentials = NewCredentials();
        (await RegisterAsync(client, credentials)).EnsureSuccessStatusCode();
        Guid sessionId = await CreateSessionAsync(client);
        (await client.PostAsJsonAsync("/auth/tokens", credentials, Ct)).EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/sessions/{sessionId}", Ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSession_OfAnotherUser_ReturnsForbidden()
    {
        // Arrange
        HttpClient victim = CreateClient();
        (await RegisterAsync(victim, NewCredentials())).EnsureSuccessStatusCode();
        Guid victimSessionId = await CreateSessionAsync(victim);

        HttpClient attacker = CreateClient();
        (await RegisterAsync(attacker, NewCredentials())).EnsureSuccessStatusCode();
        await CreateSessionAsync(attacker);

        // Act
        HttpResponseMessage response = await attacker.DeleteAsync($"/sessions/{victimSessionId}", Ct);

        // Assert
        // notes/AGENTS.md §1.2: the attacker deletes a save file they have no right to touch.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
