using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Minimal <see cref="IExceptionHandlerPathFeature"/>: the exception funnel reads the error and the
/// original path from it, nothing else.
/// </summary>
internal sealed class FakeExceptionHandlerPathFeature : IExceptionHandlerPathFeature
{
    public FakeExceptionHandlerPathFeature(Exception error, string path)
    {
        Error = error;
        Path = path;
    }

    public Exception Error { get; }

    public string Path { get; }

    public Endpoint? Endpoint { get; set; }
}
