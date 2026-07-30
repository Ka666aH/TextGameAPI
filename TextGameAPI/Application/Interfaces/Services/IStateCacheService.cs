using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface IStateCacheService
    {
        Task SetAsync(Guid sessionId, State state, CancellationToken ct = default);
        Task<State?> GetAsync(Guid sessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid sessionId, CancellationToken ct = default);
    }
}