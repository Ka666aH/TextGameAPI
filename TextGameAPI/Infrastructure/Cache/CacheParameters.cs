namespace TextGame.Infrastructure.Cache
{
    public static class CacheParameters
    {
        public static readonly TimeSpan GameSessionLifetime = TimeSpan.FromDays(7);
        public const string GameSessionStateKeyPrefix = "GameSessionState:";
    }
}
