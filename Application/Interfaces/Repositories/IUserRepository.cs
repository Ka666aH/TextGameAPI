using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task CreateAsync(User user, CancellationToken ct = default);
        Task DeleteAsync(User user, CancellationToken ct = default);
        Task<User?> GetAsync(Guid userId, CancellationToken ct = default);
        Task<User?> GetAsync(string login, CancellationToken ct = default);
    }
}