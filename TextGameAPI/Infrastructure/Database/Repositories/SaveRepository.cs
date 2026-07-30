using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class SaveRepository : ISaveRepository
    {
        private readonly AppDbContext _db;
        public SaveRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(Save save, CancellationToken ct = default) =>
            await _db.Saves.AddAsync(save, ct);
        public Task DeleteAsync(Save save, CancellationToken ct = default)
        {
            _db.Saves.Remove(save);
            return Task.CompletedTask;
        }
        public async Task<Save?> GetAsyncWithTrack(Guid saveId, CancellationToken ct = default) =>
            await _db.Saves.SingleOrDefaultAsync(x => x.Id == saveId, ct);
        public async Task<Save?> GetAsync(Guid saveId, CancellationToken ct = default) =>
            await _db.Saves.AsNoTracking().SingleOrDefaultAsync(x => x.Id == saveId, ct);

        public async Task<Save?> GetLastSaveAsync(Guid sessionId, CancellationToken ct = default) =>
        await _db.Saves
            .Where(x => x.SessionId == sessionId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default) =>
            await _db.Saves
                .AsNoTracking()
                .Where(x => x.SessionId == sessionId)
                .ToListAsync(ct);

        public async Task<Dictionary<Guid, string?>> GetAutoSaveHashesAsync(IReadOnlyList<Guid> sessionIds, CancellationToken ct)
        {
            return await _db.Saves
                .Where(x => sessionIds.Contains(x.SessionId) && x.Type == SaveType.Auto)
                .Select(x => new { x.SessionId, x.StateHash })
                .ToDictionaryAsync(x => x.SessionId, x => x.StateHash, ct);
        }

        public async Task BatchReplaceAutoSavesAsync(IReadOnlyList<Guid> sessionIds, IReadOnlyList<Save> saves, CancellationToken ct)
        {
            var existing = await _db.Saves
                .Where(x => sessionIds.Contains(x.SessionId) && x.Type == SaveType.Auto)
                .ToListAsync(ct);

            _db.Saves.RemoveRange(existing);
            await _db.Saves.AddRangeAsync(saves, ct);
        }
    }
}