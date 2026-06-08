using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface ISessionRepository
    {
        Task CreateAsync(Session gameSession, CancellationToken ct = default);
        Task DeleteAsync(Session gameSession, CancellationToken ct = default);
        Task<Session?> GetAsync (Guid gameSessionId, CancellationToken ct = default);
        Task<List<Session>> GetListAsync(Guid userId, CancellationToken ct = default);
    }
}