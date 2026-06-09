using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface ISessionRepository
    {
        Task CreateAsync(Session session, CancellationToken ct = default);
        Task DeleteAsync(Session session, CancellationToken ct = default);
        Task<Session?> GetAsync (Guid sessionId, CancellationToken ct = default);
        Task<List<Session>> GetListAsync(Guid userId, CancellationToken ct = default);
    }
}