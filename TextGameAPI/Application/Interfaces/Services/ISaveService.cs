using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISaveService
    {
        Task<Guid> CreateAsync(Guid userId, string? gameSessionName, CancellationToken ct = default);
        Task<string> LoadAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default);
        Task SaveAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default);
        Task<List<GameSession>> GetListAsync(Guid userId, CancellationToken ct = default);
    }
}