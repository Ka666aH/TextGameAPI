namespace TextGame.Infrastructure.Cache
{
    public static class CacheParameters
    {
        public static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);
        public const string StateKeyPrefix = "State:";
    }
}
