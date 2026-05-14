using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISaveService
    {
        Task<Guid> CreateGameSessionAsync(Guid userId, CancellationToken ct = default);
        Task<string> LoadGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default);
        Task SaveGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default);
        Task DeleteGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default);
        Task<List<GameSession>> GetGameSessionsAsync(Guid userId, CancellationToken ct = default);
    }
}