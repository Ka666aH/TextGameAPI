using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AutoCacheAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await next();

            if (result.Exception != null
                && result.Exception is not BattleWinException
                && result.Exception is not DefeatException
                && result.Exception is not WinException)
                return;

            var stateService = context.HttpContext.RequestServices.GetRequiredService<IStateService>();
            if (context.HttpContext.User.TryGetSessionId(out Guid sessionId))
            {
                await stateService.CacheAsync(sessionId, context.HttpContext.RequestAborted);

                // Конец партии: фоновый автосейв догнал бы его только через 5 минут, и рестарт
                // контейнера в этом окне откатил бы исход (notes/fixes-1.2.0.md, п. 11).
                if (result.Exception is DefeatException or WinException)
                    await SaveGameEndAsync(context, sessionId);
            }
        }

        private static async Task SaveGameEndAsync(ActionExecutingContext context, Guid sessionId)
        {
            var saveService = context.HttpContext.RequestServices.GetRequiredService<ISaveService>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AutoCacheAttribute>>();

            try
            {
                await saveService.SaveAutoAsync(sessionId, context.HttpContext.RequestAborted);
            }
            catch (Exception exception)
            {
                // Иначе 200 на конец партии превратится в 500 из-за сбоя записи.
                logger.LogError(exception, LoggersText.AutoSaveImmediateError, sessionId);
            }
        }
    }
}
