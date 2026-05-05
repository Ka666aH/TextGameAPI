namespace TextGame.Domain
{
    public static class TokenPolicy
    {
        private const int _refreshTokenGracePeriodMinutes = 5;
        public static DateTime GetExpiredThreshold() => DateTime.UtcNow.AddMinutes(-_refreshTokenGracePeriodMinutes);
    }
}
