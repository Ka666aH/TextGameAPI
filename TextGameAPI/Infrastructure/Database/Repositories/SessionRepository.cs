using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _db;
        public SessionRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(Session session, CancellationToken ct = default) =>
            await _db.Sessions.AddAsync(session, ct);
        public Task DeleteAsync(Session session, CancellationToken ct = default)
        {
            _db.Sessions.Remove(session);
            return Task.CompletedTask;
        }
        public async Task<Session?> GetAsync(Guid sessionId, CancellationToken ct = default) =>
            await _db.Sessions.SingleOrDefaultAsync(x => x.Id == sessionId, ct);
        public async Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default) =>
            await _db.Sessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);
    }
}