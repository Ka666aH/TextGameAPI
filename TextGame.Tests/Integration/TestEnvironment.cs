using System.Runtime.CompilerServices;

namespace TextGame.Tests.Integration;

/// <summary>
/// Variables that must exist before the first host is built, because Program.cs reads its
/// configuration eagerly (TimeSettings.Initialize, rate limiter registration, logging setup).
/// A module initializer runs earlier than any fixture constructor.
/// </summary>
internal static class TestEnvironment
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // The auth limiter allows 5 requests per minute and shares one global partition.
        Environment.SetEnvironmentVariable("TimeSettings__RateLimitPermitLimit", "1000");

        // Without this the Windows EventLog provider throws "Отказано в доступе" on every log
        // write and the AggregateException hides the real assertion result.
        Environment.SetEnvironmentVariable("Logging__EventLog__LogLevel__Default", "None");
    }
}
