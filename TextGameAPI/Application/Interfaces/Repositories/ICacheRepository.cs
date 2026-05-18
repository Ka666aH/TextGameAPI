namespace TextGame.Application.Interfaces.Repositories
{
    public interface ICacheRepository
    {
        Task SetAsync(string key, byte[] value, TimeSpan? expiration, CancellationToken ct = default);
        Task<byte[]?> GetAsync(string key, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);
    }
}