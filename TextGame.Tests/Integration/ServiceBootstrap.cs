using System.Diagnostics;
using System.Net.Sockets;

namespace TextGame.Tests.Integration;

/// <summary>
/// Makes the integration suite a one-button run: if Postgres or Redis is not listening on
/// localhost, the suite starts throwaway containers through the Docker CLI and waits for the
/// ports. The Docker API is never used, so a Docker daemon living inside WSL is fine.
///
/// Override the endpoints with TEXTGAME_TEST_POSTGRES / TEXTGAME_TEST_REDIS to skip this entirely.
/// </summary>
internal static class ServiceBootstrap
{
    private const string PostgresContainerName = "textgame-tests-postgres";
    private const string RedisContainerName = "textgame-tests-redis";
    private const int PostgresPort = 5432;
    private const int RedisPort = 6379;
    private const int WaitSeconds = 120;

    private static readonly Lazy<Task> EnsureLazy = new(EnsureCoreAsync);

    public static Task EnsureAsync() => EnsureLazy.Value;

    public static Task StopAsync() => Task.WhenAll(
        RemoveContainerAsync(PostgresContainerName),
        RemoveContainerAsync(RedisContainerName));

    private static async Task EnsureCoreAsync()
    {
        if (HasCustomEndpoints()) return;

        if (await ArePortsOpenAsync()) return;

        await RemoveContainerAsync(PostgresContainerName);
        await RemoveContainerAsync(RedisContainerName);

        // Credentials and the database name match the ConnectionStrings defaults in appsettings.json.
        await RunDockerAsync(
            "run -d --rm " +
            $"--name {PostgresContainerName} " +
            "-e POSTGRES_USER=dev_user -e POSTGRES_PASSWORD=dev_password -e POSTGRES_DB=text_game_db " +
            $"-p {PostgresPort}:5432 postgres:18",
            timeoutSeconds: 180);

        await RunDockerAsync(
            "run -d --rm " +
            $"--name {RedisContainerName} " +
            $"-p {RedisPort}:6379 redis:7-alpine",
            timeoutSeconds: 180);

        DateTime deadline = DateTime.UtcNow.AddSeconds(WaitSeconds);
        while (DateTime.UtcNow < deadline)
        {
            if (await ArePortsOpenAsync()) return;

            await Task.Delay(1000);
        }

        throw new InvalidOperationException(
            $"Postgres/Redis не отвечают на localhost:{PostgresPort}/{RedisPort}. " +
            "Подними стек вручную (`docker compose up -d postgres redis`) или задай строки подключения " +
            "в TEXTGAME_TEST_POSTGRES и TEXTGAME_TEST_REDIS.");
    }

    private static bool HasCustomEndpoints() =>
        Environment.GetEnvironmentVariable("TEXTGAME_TEST_POSTGRES") is { Length: > 0 };

    private static async Task<bool> ArePortsOpenAsync() =>
        await IsPortOpenAsync(PostgresPort) && await IsPortOpenAsync(RedisPort);

    private static async Task<bool> IsPortOpenAsync(int port)
    {
        try
        {
            using var client = new TcpClient();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await client.ConnectAsync("localhost", port, timeout.Token);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task RemoveContainerAsync(string name) =>
        await RunDockerAsync($"rm -f {name}", timeoutSeconds: 30);

    private static async Task RunDockerAsync(string arguments, int timeoutSeconds)
    {
        try
        {
            var startInfo = new ProcessStartInfo("docker", arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using Process? process = Process.Start(startInfo);
            if (process is null) return;

            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> error = process.StandardError.ReadToEndAsync();

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            await process.WaitForExitAsync(timeout.Token);
            await Task.WhenAll(output, error);
        }
        catch
        {
            // docker missing, daemon asleep or image not present: the port probe decides what happens next.
        }
    }
}
