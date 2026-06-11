namespace TextGame.Infrastructure.Configuration
{
    public static class TimeSettings
    {
        public static TimeSpan AccessTokenLifetime { get; private set; } = TimeSpan.FromMinutes(15);
        public static TimeSpan RefreshTokenLifetime { get; private set; } = TimeSpan.FromDays(14);
        public static TimeSpan AccessCookieLifetime { get; private set; } = TimeSpan.FromDays(14);
        public static TimeSpan RefreshCookieLifetime { get; private set; } = TimeSpan.FromDays(14);
        public static TimeSpan StateCacheLifetime { get; private set; } = TimeSpan.FromMinutes(10);
        public static TimeSpan AutoSaveInterval { get; private set; } = TimeSpan.FromMinutes(5);
        public static TimeSpan RateLimitWindow { get; private set; } = TimeSpan.FromMinutes(1);
        public static int RateLimitPermitLimit { get; private set; } = 5;
        public static int RateLimitQueueLimit { get; private set; } = 0;

        public static void Initialize(IConfiguration configuration)
        {
            var section = configuration.GetSection("TimeSettings");

            if (TimeSpan.TryParse(section["AccessTokenLifetime"], out var accessToken))
                AccessTokenLifetime = accessToken;
            if (TimeSpan.TryParse(section["RefreshTokenLifetime"], out var refreshToken))
                RefreshTokenLifetime = refreshToken;
            if (TimeSpan.TryParse(section["AccessCookieLifetime"], out var accessCookie))
                AccessCookieLifetime = accessCookie;
            if (TimeSpan.TryParse(section["RefreshCookieLifetime"], out var refreshCookie))
                RefreshCookieLifetime = refreshCookie;
            if (TimeSpan.TryParse(section["StateCacheLifetime"], out var stateCache))
                StateCacheLifetime = stateCache;
            if (TimeSpan.TryParse(section["AutoSaveInterval"], out var autoSave))
                AutoSaveInterval = autoSave;
            if (TimeSpan.TryParse(section["RateLimitWindow"], out var rateWindow))
                RateLimitWindow = rateWindow;
            if (int.TryParse(section["RateLimitPermitLimit"], out var permitLimit))
                RateLimitPermitLimit = permitLimit;
            if (int.TryParse(section["RateLimitQueueLimit"], out var queueLimit))
                RateLimitQueueLimit = queueLimit;
        }
    }
}
