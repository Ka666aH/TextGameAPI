using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextGame.Application.DTO;
using TextGame.Application.Services;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Presentation.Controllers;
using TextGame.Presentation.DTO;
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
    public void Handle_WhenInvalidIdRejectsTheRequest_Returns403()
    {
        // Arrange
        var exception = new InvalidIdException("INVALID_ID", "нет такого идентификатора");
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/state/rooms/42");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        // notes/AGENTS.md, §4.2: a wrong id is a refused game action, so the contract says 403,
        // not 400 (the id was syntactically fine) and not 422.
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void Handle_WhenItemIsUnsellable_Returns403()
    {
        // Arrange
        var exception = new UnsellableItemException();
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/state/inventory/7/sell");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        // notes/AGENTS.md, §4.2: 422 stays unused on purpose, a refused sale is a 403 either way.
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void Handle_WhenSessionClaimIsMissing_Returns403AndKeepsTheAuthCookies()
    {
        // Arrange
        var exception = new MissingSessionIdClaimException();
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Put, "/sessions/current");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        // «Нет активной игры» — не сломанная авторизация: куки не удаляем, иначе выбор
        // сессии выбрасывает залогиненного игрока на экран логина.
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        controller.HttpContext.Response.Headers.SetCookie.ToString().Should().BeEmpty();
        logger.Entries[0].Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Handle_WhenValidationFails_Returns400AndKeepsTheLogAtDebug()
    {
        // Arrange
        var exception = new ValidationException([new ValidationFailure("Login", "слишком короткий")]);
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/users");

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

    [Fact]
    public void Handle_WhenBattleIsWon_Returns200WithBattleLogAndGameInfo()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        GameInfoDTO gameInfo = new GameInfoService(new FakeStateService(state)).GetGameInfo();
        var battleLog = new BattleLog("Противник", 3, 10, 7, "ИГРОК", 0, 10, 10);
        var exception = new BattleWinException("Противник повержен.", battleLog, gameInfo);
        var logger = new RecordingLogger<ExceptionController>();
        var controller = CreateController(logger, exception, HttpMethods.Post, "/state/rooms/current/enemy/attack");

        // Act
        IActionResult result = controller.Handle();

        // Assert
        // notes/backend-requests.md п. 9: gameInfo приходит в том же ответе, клиенту не нужен
        // обязательный GET /state после каждой победы.
        var dto = result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<BattleWinDTO>().Which;
        dto.BattleLog.Should().BeSameAs(battleLog);
        dto.GameInfo.Should().BeSameAs(gameInfo);
        dto.GameInfo.IsGameStarted.Should().BeTrue();
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
