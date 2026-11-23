using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using TextGame.Domain.GameText;

namespace TextGame.Presentation.Authorization
{
    /// <summary>
    /// 401/403 с телом ProblemDetails вместо пустого ответа middleware.
    /// notes/backend-requests.md п. 13: клиент не должен угадывать причину по пустому телу.
    /// </summary>
    public class ProblemDetailsAuthorizationHandler : IAuthorizationMiddlewareResultHandler
    {
        // Явные web-опции: тело должно быть camelCase, как у остальных ProblemDetails MVC.
        private static readonly JsonSerializerOptions ProblemJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Challenged)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    ExceptionsLabels.AccessTokenNotFoundCode,
                    ExceptionsLabels.AccessTokenNotFoundMessage);
                return;
            }

            if (authorizeResult.Forbidden)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ExceptionsLabels.MissingSessionIdClaimCode,
                    ExceptionsLabels.MissingSessionIdClaimMessage);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
        {
            // 401 без WWW-Authenticate ломает клиентов, которые ждут стандартный challenge.
            if (statusCode == StatusCodes.Status401Unauthorized)
                context.Response.Headers["WWW-Authenticate"] = "Bearer";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem, ProblemJsonOptions, "application/problem+json", context.RequestAborted);
        }
    }
}
