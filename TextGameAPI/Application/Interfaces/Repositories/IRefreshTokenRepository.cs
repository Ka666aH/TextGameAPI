using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken refreshToken, CancellationToken ct = default);
        Task DeleteAsync(RefreshToken refreshToken, CancellationToken ct = default);
        Task DeleteAllAsync(Guid userId, CancellationToken ct = default);
        Task<RefreshToken?> GetAsync(string token, CancellationToken ct = default);
    }
}