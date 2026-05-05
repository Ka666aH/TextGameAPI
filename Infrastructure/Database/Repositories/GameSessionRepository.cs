using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly AppDbContext _db;
        public GameSessionRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(GameSession gameSession, CancellationToken ct = default) => 
            await _db.GameSessions.AddAsync(gameSession, ct);

        public Task DeleteAsync(GameSession gameSession, CancellationToken ct = default)
        {
            _db.GameSessions.Remove(gameSession);
            return Task.CompletedTask;
        }

        public async Task<GameSession?> GetAsync(Guid gameSessionId, CancellationToken ct = default) =>
            await _db.GameSessions
            .SingleOrDefaultAsync(x => x.Id == gameSessionId, ct);

        public async Task<List<GameSession>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
            await _db.GameSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);
    }
}