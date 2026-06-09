namespace TextGame.Presentation.Options
{
    public static class RateLimiter
    {
        public const string AuthPolicyName = "Auth";
        public const int AuthPermitLimit = 5;
        public static TimeSpan AuthWindow = TimeSpan.FromMinutes(1);
        public const int AuthQueueLimit = 0;
    }
}
