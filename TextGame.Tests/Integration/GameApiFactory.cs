using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using TextGame.Infrastructure.Database;
using TextGame.Presentation.DTO;

namespace TextGame.Tests.Integration;

/// <summary>
/// Starts the real API pipeline against the services the compose stack already exposes
/// (Postgres on localhost:5432, Redis on localhost:6379 — the same values as appsettings.json).
///
/// <see cref="AppDbContext"/> is used only as a handle on the TextGame assembly: the entry point
/// stays the generated Program, this is just the public type the factory can name. No production
/// file has to be touched to make the app testable.
/// </summary>
public class GameApiFactory : WebApplicationFactory<AppDbContext>
{
    static GameApiFactory()
    {
        // Program.cs reads its configuration before the test host is built (TimeSettings.Initialize,
        // UseNpgsql, ConnectionMultiplexer.Connect), so overrides have to be visible as environment
        // variables. ConfigureAppConfiguration arrives too late for those call sites.
        Environment.SetEnvironmentVariable("TimeSettings__RateLimitPermitLimit", "1000");

        if (Environment.GetEnvironmentVariable("TEXTGAME_TEST_POSTGRES") is { Length: > 0 } postgres)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSQL", postgres);
        }

        if (Environment.GetEnvironmentVariable("TEXTGAME_TEST_REDIS") is { Length: > 0 } redis)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Redis", redis);
        }
    }

    public GameApiFactory() =>
        // Sync-over-async on purpose: the fixture must be ready before the first client is created.
        ServiceBootstrap.EnsureAsync().GetAwaiter().GetResult();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Never write to the Windows event log from tests: without administrator rights the
        // provider throws and that exception replaces the real one under test.
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await ServiceBootstrap.StopAsync();
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<GameApiFactory>
{
    public const string Name = "api";
}

/// <summary>
/// Shared plumbing for the end-to-end suite. Every client owns its own cookie jar, so two
/// clients are two independent users.
/// </summary>
public abstract class IntegrationTestBase
{
    protected IntegrationTestBase(GameApiFactory factory) => Factory = factory;

    protected GameApiFactory Factory { get; }

    protected HttpClient CreateClient() => Factory.CreateClient();

    protected static AuthRequest NewCredentials() => new($"user_{Guid.NewGuid():N}", "Pass1");

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    protected static async Task<HttpResponseMessage> RegisterAsync(HttpClient client, AuthRequest credentials) =>
        await client.PostAsJsonAsync("/users", credentials, Ct);

    protected static async Task<Guid> CreateSessionAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsync("/sessions", content: null, Ct);
        response.EnsureSuccessStatusCode();

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);

        return body.GetProperty("sessionId").GetGuid();
    }
}
