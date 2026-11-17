using Microsoft.Extensions.Logging;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Captures log entries instead of writing them anywhere, so a test can assert on level, rendered
/// message and the attached exception without standing up a logging stack.
/// </summary>
internal sealed class RecordingLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
}

internal sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
