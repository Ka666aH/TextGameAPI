using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken refreshToken, CancellationToken ct = default);
        Task RevokeAsync(RefreshToken refreshToken, CancellationToken ct = default);
        Task RevokeAllAsync(Guid userId, CancellationToken ct = default);
        Task<int> DeleteExpiredAsync(CancellationToken ct = default);
        Task<RefreshToken?> GetAsync(string token, CancellationToken ct = default);
    }
}