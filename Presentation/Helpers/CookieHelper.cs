using TextGame.Infrastructure.Token;

namespace TextGame.Presentation.Helpers
{
    public static class CookieHelper
    {
        public static void SetRefreshCookie(this HttpResponse response, string value) =>
            response.Cookies.Append(Parameters.RefreshToken, value);
        public static void SetAccessCookie(this HttpResponse response, string value) =>
            response.Cookies.Append(Parameters.AccessToken, value);
        public static void DeleteAuthCookies(this HttpResponse response)
        {
            response.Cookies.Delete(Parameters.RefreshToken);
            response.Cookies.Delete(Parameters.AccessToken);
        }
    }
}
