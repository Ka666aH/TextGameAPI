namespace TextGame.Application.Interfaces.Repositories
{
    public interface ICacheRepository
    {
        Task SetAsync(string key, string value, TimeSpan? expiration, CancellationToken ct = default);
        Task<string?> GetAsync(string key, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);
    }
}