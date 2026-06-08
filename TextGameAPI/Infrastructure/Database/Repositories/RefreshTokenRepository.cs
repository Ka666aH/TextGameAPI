using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;
        public RefreshTokenRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
            await _db.RefreshTokens.AddAsync(refreshToken, ct);

        public async Task<RefreshToken?> GetAsync(string token, CancellationToken ct = default) =>
            await _db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token, ct);

        public async Task DeleteAllAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync(ct);
            _db.RemoveRange(tokens);
        }

        public Task DeleteAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            _db.RefreshTokens.Remove(refreshToken);
            return Task.CompletedTask;
        }
    }
}