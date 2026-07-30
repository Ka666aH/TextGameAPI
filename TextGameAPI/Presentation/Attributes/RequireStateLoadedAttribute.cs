using Microsoft.AspNetCore.Mvc.Filters;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireStateLoadedAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stateService = context.HttpContext.RequestServices.GetRequiredService<IStateService>();
            if (context.HttpContext.User.TryGetSessionId(out Guid sessionId))
            {
                await stateService.EnsureLoadedAsync(sessionId, context.HttpContext.RequestAborted);
            }

            await next();
        }
    }
}
