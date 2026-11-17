namespace TextGame.Application.Interfaces.Repositories
{
    public interface ICacheRepository
    {
        Task SetAsync(string key, string value, TimeSpan? expiration, CancellationToken ct = default);
        Task<string?> GetAsync(string key, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Returns every key that matches <paramref name="pattern"/>, walking the keyspace page by page.
        /// The result may include keys created while the walk was running; it never contains duplicates.
        /// </summary>
        Task<IReadOnlyList<string>> GetKeysAsync(string pattern, CancellationToken ct = default);

        /// <summary>
        /// Returns the values of the requested keys. Keys that no longer exist are absent from the result.
        /// </summary>
        Task<IReadOnlyDictionary<string, string>> GetManyAsync(IReadOnlyCollection<string> keys, CancellationToken ct = default);
    }
}
