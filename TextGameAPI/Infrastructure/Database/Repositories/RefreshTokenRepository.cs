using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;
using TextGame.Infrastructure.Token;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;
        public RefreshTokenRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
            await _db.RefreshTokens.AddAsync(refreshToken, ct);
        public async Task<int> DeleteExpiredAsync(CancellationToken ct = default) =>
            await _db.RefreshTokens
            .Where(x => x.ExpiresUTC < DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);

        public async Task<RefreshToken?> GetAsync(string token, CancellationToken ct = default) =>
            await _db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token, ct);

        public async Task RevokeAllAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _db.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync(ct);
            foreach(var token in tokens)
            {
                ct.ThrowIfCancellationRequested();
                token.Revoke();
            }
        }

        public Task RevokeAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            refreshToken.Revoke();
            return Task.CompletedTask;
        }
    }
}