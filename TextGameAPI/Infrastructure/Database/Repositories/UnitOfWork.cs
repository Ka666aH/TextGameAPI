using TextGame.Application.Interfaces.Repositories;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public UnitOfWork(AppDbContext db) => _db = db;
        public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
        public void ClearChangeTracker() => _db.ChangeTracker.Clear();
    }
}