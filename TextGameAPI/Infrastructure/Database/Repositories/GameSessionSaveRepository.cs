using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class GameSessionSaveRepository : IGameSessionSaveRepository
    {
        private readonly AppDbContext _db;
        public GameSessionSaveRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(GameSessionSave gameSessionSave, CancellationToken ct = default) =>
            await _db.GameSessionSaves.AddAsync(gameSessionSave, ct);
        public Task DeleteAsync(GameSessionSave gameSessionSave, CancellationToken ct = default)
        {
            _db.GameSessionSaves.Remove(gameSessionSave);
            return Task.CompletedTask;
        }
        public async Task<GameSessionSave?> GetAsyncWithTrack(Guid gameSessionSaveId, CancellationToken ct = default) =>
            await _db.GameSessionSaves.SingleOrDefaultAsync(x => x.Id == gameSessionSaveId, ct);
        public async Task<GameSessionSave?> GetAsync(Guid gameSessionSaveId, CancellationToken ct = default) =>
            await _db.GameSessionSaves.AsNoTracking().SingleOrDefaultAsync(x => x.Id == gameSessionSaveId, ct);

        public async Task<GameSessionSave?> GetLastSaveAsync(Guid gameSessionId, CancellationToken ct = default) =>
        await _db.GameSessionSaves
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        public async Task<List<GameSessionSave>> GetListAsync(Guid gameSessionId, CancellationToken ct = default) =>
            await _db.GameSessionSaves
                .AsNoTracking()
                .Where(x => x.GameSessionId == gameSessionId)
                .ToListAsync(ct);

        public async Task<Dictionary<Guid, string?>> GetAutoSaveHashesAsync(List<Guid> sessionIds, CancellationToken ct)
        {
            return await _db.GameSessionSaves
                .Where(x => sessionIds.Contains(x.GameSessionId) && x.Type == SaveType.Auto)
                .Select(x => new { x.GameSessionId, x.StateHash })
                .ToDictionaryAsync(x => x.GameSessionId, x => x.StateHash, ct);
        }

        public async Task BatchReplaceAutoSavesAsync(List<Guid> sessionIds, List<GameSessionSave> saves, CancellationToken ct)
        {
            var existing = await _db.GameSessionSaves
                .Where(x => sessionIds.Contains(x.GameSessionId) && x.Type == SaveType.Auto)
                .ToListAsync(ct);

            _db.GameSessionSaves.RemoveRange(existing);
            await _db.GameSessionSaves.AddRangeAsync(saves, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}