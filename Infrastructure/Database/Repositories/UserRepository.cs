using Microsoft.EntityFrameworkCore;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Domain.Entities;

namespace TextGame.Infrastructure.Database.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;
        public async Task CreateAsync(User user, CancellationToken ct = default) => 
            await _db.Users.AddAsync(user, ct);

        public Task DeleteAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Remove(user);
            return Task.CompletedTask;
        }

        public async Task<User?> GetAsync(Guid userId, CancellationToken ct = default) => 
            await _db.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken: ct);

        public async Task<User?> GetAsync(string login, CancellationToken ct = default) => 
            await _db.Users.SingleOrDefaultAsync(x => x.Login == login, ct);
    }
}