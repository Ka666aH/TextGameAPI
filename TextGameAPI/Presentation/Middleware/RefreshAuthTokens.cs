using System.IdentityModel.Tokens.Jwt;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Middleware
{
    public class RefreshAuthTokens
    {
        private readonly RequestDelegate _next;
        
        public RefreshAuthTokens(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            //Проверить нужно ли обновлять
            if (context.GetEndpoint()?.Metadata.GetMetadata<BypassRefreshAttribute>() is not null) { await _next(context); return; }

            //Получить необходимые сервисы
            var authService = context.RequestServices.GetRequiredService<IAuthService>();
            //Проверить срок жизни токена
            bool accessTokenExist = context.Request.TryGetAccessToken(out string accessToken);
            if (accessTokenExist)
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    await _next(context);
                    return;
                }
            }
            //Получить остальные данные
            string refreshToken = GetRefreshToken(context);
            string fingerprint = GetFingerprint(context);
            //Обновить в сервисе
            var refreshResult = await authService.RefreshAsync(refreshToken, accessToken, fingerprint);
            //Установить новые куки в запрос и ответ
            string newCookieHeader = $"{TokenParameters.AccessToken}={refreshResult.AccessToken}; " +
                                     $"{TokenParameters.RefreshToken}={refreshResult.RefreshToken}";
            context.Request.Headers.Cookie = newCookieHeader;

            CookieHelper.SetAuthCookies(refreshResult, context);
            context.Items[ItemKeys.RefreshResultKey] = refreshResult;
            await _next(context);
        }
        private static string GetFingerprint(HttpContext context)
        {
            string deviceName = context.Request.Headers.UserAgent.ToString();
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
            return deviceName + " | " + ip;
        }
        private static string GetRefreshToken(HttpContext context)
        {
            var request = context.Request;
            bool refreshTokenExist = request.TryGetRefreshToken(out string refreshToken);
            if (!refreshTokenExist) throw new RefreshTokenNotFoundException();
            return refreshToken;
        }
    }
    public static class RefreshAuthTokensExtensions
    {
        public static IApplicationBuilder UseRefreshAuthTokens(this IApplicationBuilder app)
            => app.UseMiddleware<RefreshAuthTokens>();
    }
}