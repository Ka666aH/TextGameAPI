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
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };
        private static CookieOptions SetExpiringOptions(TimeSpan tokenLifetime) =>
            new()
            {
                HttpOnly = DefaultOptions.HttpOnly,
                Secure = DefaultOptions.Secure,
                SameSite = DefaultOptions.SameSite,
                Path = DefaultOptions.Path,
                Expires = DateTimeOffset.UtcNow.Add(tokenLifetime)
            };
        public static void SetRefreshCookie(this HttpResponse response, string value) =>
                response.Cookies.Append(TokenParameters.RefreshToken, value, SetExpiringOptions(TimeSettings.RefreshCookieLifetime));
        public static void SetAccessCookie(this HttpResponse response, string value) =>
            response.Cookies.Append(TokenParameters.AccessToken, value, SetExpiringOptions(TimeSettings.AccessCookieLifetime));
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
