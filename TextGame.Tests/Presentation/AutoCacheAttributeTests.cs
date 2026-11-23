using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Attributes;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Presentation;

/// <summary>
/// notes/fixes-1.2.0.md, п. 11: конец партии пишет автосейв сразу, а не ждёт фоновый тик.
/// Ошибка записи не должна ломать ответ на игровое действие.
/// </summary>
public class AutoCacheAttributeTests
{
    [Fact]
    public async Task OnActionExecutionAsync_WhenGameEnds_TriggersImmediateAutoSave()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var saveService = new RecordingSaveService();
        var context = CreateContext(sessionId, saveService);

        // Act
        await new AutoCacheAttribute().OnActionExecutionAsync(
            context.Executing,
            () => Task.FromResult(context.Executed(new WinException(null!))));

        // Assert
        saveService.AutoSavedSessionIds.Should().ContainSingle().Which.Should().Be(sessionId);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenBattleIsWon_DoesNotTriggerAutoSave()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var saveService = new RecordingSaveService();
        var context = CreateContext(sessionId, saveService);

        // Act
        await new AutoCacheAttribute().OnActionExecutionAsync(
            context.Executing,
            () => Task.FromResult(context.Executed(new BattleWinException("победа", null!, null!))));

        // Assert
        // Победа в бою — не конец партии: кеширования достаточно.
        saveService.AutoSavedSessionIds.Should().BeEmpty();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenSaveFails_DoesNotThrow()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var saveService = new RecordingSaveService { ThrowOnSave = true };
        var context = CreateContext(sessionId, saveService);

        // Act
        Func<Task> act = () => new AutoCacheAttribute().OnActionExecutionAsync(
            context.Executing,
            () => Task.FromResult(context.Executed(new DefeatException("фиаско", null!))));

        // Assert
        // Ошибку записи логируем и глотаем: иначе 200 на конец партии стал бы 500.
        await act.Should().NotThrowAsync();
    }

    private static AttributeContext CreateContext(Guid sessionId, RecordingSaveService saveService)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IStateService>(new FakeStateService(new State()));
        services.AddSingleton<ISaveService>(saveService);

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        var identity = new ClaimsIdentity("test");
        identity.AddClaim(new Claim(AccessClaims.SessionId, sessionId.ToString()));
        httpContext.User = new ClaimsPrincipal(identity);

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var controller = new object();
        var executing = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), controller);

        return new AttributeContext(
            executing,
            exception => new ActionExecutedContext(actionContext, [], controller) { Exception = exception });
    }

    private sealed record AttributeContext(
        ActionExecutingContext Executing,
        Func<Exception, ActionExecutedContext> Executed);

    private sealed class RecordingSaveService : ISaveService
    {
        public List<Guid> AutoSavedSessionIds { get; } = [];
        public bool ThrowOnSave { get; init; }

        public Task SaveAutoAsync(Guid sessionId, CancellationToken ct = default)
        {
            if (ThrowOnSave) throw new InvalidOperationException("база недоступна");

            AutoSavedSessionIds.Add(sessionId);
            return Task.CompletedTask;
        }

        public Task<Guid> CreateAsync(Guid sessionId, SaveType saveType, string? name = null, long? seed = null, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task LoadAsync(Guid sessionId, Guid saveId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task DeleteAsync(Guid sessionId, Guid saveId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
