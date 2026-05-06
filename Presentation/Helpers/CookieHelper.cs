using TextGame.Infrastructure.Token;

namespace TextGame.Presentation.Helpers
{
    public static class CookieHelper
    {
        private static readonly CookieOptions DefaultOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
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
                response.Cookies.Append(TokenParameters.RefreshToken, value, SetExpiringOptions(TokenParameters.RefreshTokenLifetime));
        public static void SetAccessCookie(this HttpResponse response, string value) =>
            response.Cookies.Append(TokenParameters.AccessToken, value, SetExpiringOptions(TokenParameters.RefreshTokenLifetime));

        public static void DeleteAuthCookies(this HttpResponse response)
        {
            response.Cookies.Delete(TokenParameters.RefreshToken, DefaultOptions);
            response.Cookies.Delete(TokenParameters.AccessToken, DefaultOptions);
        }
    }
}
