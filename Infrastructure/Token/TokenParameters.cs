namespace TextGame.Infrastructure.Token
{
    public static class TokenParameters
    {
        public const string AccessToken = "AccessToken";
        public const string RefreshToken = "RefreshToken";

        public const string Issuer = "TextGameAPI";
        public const string Audience = "TextGameClient";

        public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(10);

        private const int _refreshTokenGracePeriodMinutes = 10;
        public static DateTime GetExpiredThreshold() => DateTime.UtcNow.AddMinutes(-_refreshTokenGracePeriodMinutes);
    }
}
