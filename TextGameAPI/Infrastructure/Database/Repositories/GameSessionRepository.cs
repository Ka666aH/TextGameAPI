using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Items.Other;

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
            .AsNoTracking()
            .IncludeFullGameData()
            .SingleOrDefaultAsync(x => x.Id == gameSessionId, ct);

        public async Task<List<GameSession>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
            await _db.GameSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        public async Task<GameSession?> GetWithTrackingAsync(Guid gameSessionId, CancellationToken ct = default) =>
            await _db.GameSessions
            .IncludeFullGameData()
            .SingleOrDefaultAsync(x => x.Id == gameSessionId, ct);

    }
    public static class GameSessionRepositoryExtension
    {
        public static IQueryable<GameSession> IncludeFullGameData(this IQueryable<GameSession> query) => 
            query
            .Include(g => g.Rooms).ThenInclude(r => r.Enemies)
            .Include(g => g.Rooms).ThenInclude(r => r.Items)
            .Include(g => g.Rooms).ThenInclude(r => r.Items.OfType<Chest>()).ThenInclude(c => c.Items)
            .Include(g => g.Inventory)
            .Include(g => g.CurrentRoom).ThenInclude(r => r!.Enemies)
            .Include(g => g.CurrentRoom).ThenInclude(r => r!.Items)
            .Include(g => g.CurrentRoom).ThenInclude(r => r!.Items.OfType<Chest>()).ThenInclude(c => c.Items)
            .Include(g => g.Weapon)
            .Include(g => g.Helm)
            .Include(g => g.Chestplate)
            .Include(g => g.CurrentMimicChest).ThenInclude(c => c!.Items);
    }
}