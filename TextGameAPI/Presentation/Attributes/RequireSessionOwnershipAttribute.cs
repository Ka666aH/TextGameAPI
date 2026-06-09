using Microsoft.AspNetCore.Mvc.Filters;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireSessionOwnershipAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var guard = context.HttpContext.RequestServices.GetRequiredService<ISessionAccessGuard>();
            if (context.HttpContext.User.TryGetSessionId(out Guid sessionId))
            {
                await guard.EnsureOwnershipAsync(sessionId, context.HttpContext.RequestAborted);
            }
        }
    }
}