using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Presentation.Controllers;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Presentation;

/// <summary>
/// notes/AGENTS.md §2.8: every exception of the pipeline ends up in this one action, so it is the
/// single place where a request can leave a trace. The tests pin what gets logged and at what level —
/// 5xx with the stack, game refusals without it, and normal play out of the error path.
/// </summary>
public class ExceptionControllerTests
{
    [Fact]
    public void Handle_WhenExceptionIsUnknown_Returns500AndLogsErrorWithTheException()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/state/battle/attack");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        logger.Entries.Should().ContainSingle();
        LogEntry entry = logger.Entries[0];
        entry.Level.Should().Be(LogLevel.Error);
        entry.Exception.Should().BeSameAs(exception);
        entry.Message.Should().Contain("POST");
        entry.Message.Should().Contain("/state/battle/attack");
        entry.Message.Should().Contain("500");
    }

    [Fact]
    public void Handle_WhenGameRuleRejectsTheRequest_Returns403AndLogsWarningWithoutStack()
    {
        // Arrange
        var exception = new ClosedException();
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/state/rooms/3");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        logger.Entries.Should().ContainSingle();
        LogEntry entry = logger.Entries[0];
        entry.Level.Should().Be(LogLevel.Warning);
        entry.Exception.Should().BeNull("a refused move is expected play, its stack trace is noise");
        entry.Message.Should().Contain(ExceptionsLabels.ClosedCode);
    }

    [Fact]
    public void Handle_WhenValidationFails_Returns400AndKeepsTheLogAtDebug()
    {
        // Arrange
        var exception = new ValidationException([new ValidationFailure("Login", "слишком короткий")]);
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/auth/register");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Debug);
        logger.Entries[0].Message.Should().Contain(ExceptionsLabels.ValidationErrorCode);
    }

    [Fact]
    public void Handle_WhenExceptionFeatureIsMissing_Returns500AndLogsError()
    {
        // Arrange
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception: null, HttpMethods.Get, "/state");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Error);
        logger.Entries[0].Exception.Should().BeNull();
    }

    private static ExceptionController CreateController(
        ILogger<ExceptionController> logger, Exception? exception, string method, string path)
    {
        // ControllerBase.Problem resolves ProblemDetailsFactory from the request services.
        var services = new ServiceCollection();
        services.AddControllers();

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Method = method;
        httpContext.Request.Path = path;

        if (exception is not null)
            httpContext.Features.Set<IExceptionHandlerPathFeature>(new FakeExceptionHandlerPathFeature(exception, path));

        return new ExceptionController(logger)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };
    }
}
