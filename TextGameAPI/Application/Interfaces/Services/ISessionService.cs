using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISessionService
    {
        Task<Guid> CreateAsync(Guid userId, string? sessionName, CancellationToken ct = default);
        Task<string> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid sessionId, CancellationToken ct = default);
        Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default);
    }
}