using Microsoft.AspNetCore.Mvc.Filters;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.Helpers;

namespace TextGame.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireSessionOwnershipAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string SessionIdRouteKey = "sessionId";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Без клейма продолжать нельзя: раньше фильтр молча пропускал такой запрос.
            if (!context.HttpContext.User.TryGetSessionId(out Guid claimedSessionId))
                throw new MissingSessionIdClaimException();

            // Проверяем сессию из роута, а не из клейма: иначе проверялась своя сессия и владелец
            // чужой не проверялся вообще (IDOR).
            Guid sessionIdToCheck = TryGetRouteSessionId(context, out Guid routeSessionId)
                ? routeSessionId
                : claimedSessionId;

            var guard = context.HttpContext.RequestServices.GetRequiredService<ISessionAccessGuard>();
            await guard.EnsureOwnershipAsync(sessionIdToCheck, context.HttpContext.RequestAborted);
        }

        private static bool TryGetRouteSessionId(AuthorizationFilterContext context, out Guid sessionId)
        {
            sessionId = default;
            if (!context.RouteData.Values.TryGetValue(SessionIdRouteKey, out object? rawValue)) return false;

            return Guid.TryParse(rawValue?.ToString(), out sessionId);
        }
    }
}
