using Microsoft.AspNetCore.Mvc.Filters;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
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
            }
        }
    }
}