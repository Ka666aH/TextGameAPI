namespace TextGame.Infrastructure.Cache
{
    public static class CacheParameters
    {
        public static readonly TimeSpan GameSessionLifetime = TimeSpan.FromMinutes(10);
        public const string GameSessionStateKeyPrefix = "GameSessionState:";
    }
}
