using System.Net;

namespace TextGame.Tests.Integration;

/// <summary>
/// Two real users over real HTTP. Both red tests are the IDOR from notes/AGENTS.md §1.2: the
/// ownership filter validates the session stored in the caller's own claim, so the session id
/// from the route is never checked against the owner.
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
        HttpResponseMessage response = await client.PostAsync($"/sessions/{sessionId}", content: null, Ct);

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
        HttpResponseMessage response = await attacker.PostAsync($"/sessions/{victimSessionId}", content: null, Ct);

        // Assert
        // notes/AGENTS.md §1.2: the attacker gets an access token for a session they do not own.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
