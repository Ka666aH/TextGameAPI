using System.IdentityModel.Tokens.Jwt;
using TextGame.Application.DTO;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.Token;
using TextGame.Presentation.Attributes;
using TextGame.Presentation.Helpers;

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
            var tokenRepo = context.RequestServices.GetRequiredService<ITokenRepository>();
            var authService = context.RequestServices.GetRequiredService<IAuthService>();
            //Проверить срок жизни токена
            bool accessTokenExist = context.Request.TryGetAccessToken(out string accessToken);
            if (accessTokenExist)
            {
                var principal = tokenRepo.ReadTokenWithoutLifetime(accessToken);
                if (principal == null) { await _next(context); return; }

                var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                if (expClaim == null) { await _next(context); return; }

                var expUnix = long.Parse(expClaim);
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                if (expTime > DateTime.UtcNow) { await _next(context); return; }
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

            SetAuthCookies(refreshResult, context);

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
        private static void SetAuthCookies(AuthResult authResult, HttpContext context)
        {
            var response = context.Response;
            response.SetRefreshCookie(authResult.RefreshToken);
            response.SetAccessCookie(authResult.AccessToken);
        }
    }
    public static class RefreshAuthTokensExtensions
    {
        public static IApplicationBuilder UseRefreshAuthTokens(this IApplicationBuilder app)
            => app.UseMiddleware<RefreshAuthTokens>();
    }
}
