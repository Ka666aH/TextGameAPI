using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface IStateCacheService
    {
        Task SetAsync(Guid gameSessionId, State gameSessionState, CancellationToken ct = default);
        Task<State?> GetAsync(Guid gameSessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}