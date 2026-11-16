using TextGame.Application.DTO;
using TextGame.Infrastructure.Configuration;
using TextGame.Infrastructure.Token;

namespace TextGame.Presentation.Helpers
{
    public static class CookieHelper
    {
        private static readonly CookieOptions DefaultOptions = new()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };
        private static CookieOptions SetExpiringOptions(TimeSpan tokenLifetime, bool secure) =>
            new()
            {
                HttpOnly = DefaultOptions.HttpOnly,
                Secure = secure,
                SameSite = DefaultOptions.SameSite,
                Path = DefaultOptions.Path,
                Expires = DateTimeOffset.UtcNow.Add(tokenLifetime)
            };
        // notes/AGENTS.md §2.9: the flag follows the actual scheme instead of being hardcoded,
        // so an HTTPS deployment never emits a cookie that can travel over plain HTTP.
        private static bool IsSecureRequest(HttpResponse response) => response.HttpContext.Request.IsHttps;
        public static void SetRefreshCookie(this HttpResponse response, string value) =>
                response.Cookies.Append(TokenParameters.RefreshToken, value, SetExpiringOptions(TimeSettings.RefreshCookieLifetime, IsSecureRequest(response)));
        public static void SetAccessCookie(this HttpResponse response, string value) =>
            response.Cookies.Append(TokenParameters.AccessToken, value, SetExpiringOptions(TimeSettings.AccessCookieLifetime, IsSecureRequest(response)));
        public static void SetAuthCookies(AuthResult authResult, HttpContext context)
        {
            var response = context.Response;
            response.SetRefreshCookie(authResult.RefreshToken);
            response.SetAccessCookie(authResult.AccessToken);
        }
        public static bool TryGetRefreshToken(this HttpRequest request, out string refreshToken)
        {
            refreshToken = request.Cookies[TokenParameters.RefreshToken] ?? "";
            return refreshToken != "";
        }
        public static bool TryGetAccessToken(this HttpRequest request, out string accessToken)
        {
            accessToken = request.Cookies[TokenParameters.AccessToken] ?? "";
            return accessToken != "";
        }
        public static void DeleteAuthCookies(this HttpResponse response)
        {
            response.Cookies.Delete(TokenParameters.RefreshToken, DefaultOptions);
            response.Cookies.Delete(TokenParameters.AccessToken, DefaultOptions);
        }
    }
}
