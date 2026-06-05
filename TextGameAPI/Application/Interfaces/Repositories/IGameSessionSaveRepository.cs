using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface IGameSessionSaveRepository
    {
        Task CreateAsync(GameSessionSave gameSessionSave, CancellationToken ct = default);
        Task<GameSessionSave?> GetAsyncWithTrack(Guid gameSessionSaveId, CancellationToken ct = default);
        Task<GameSessionSave?> GetAsync(Guid gameSessionSaveId, CancellationToken ct = default);
        Task<GameSessionSave?> GetLastSaveAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<List<GameSessionSave>> GetListAsync(Guid gameSessionId, CancellationToken ct = default);
        Task DeleteAsync(GameSessionSave gameSessionSave, CancellationToken ct = default);

        Task<Dictionary<Guid, string?>> GetAutoSaveHashesAsync(List<Guid> sessionIds, CancellationToken ct);
        Task BatchReplaceAutoSavesAsync(List<Guid> sessionIds, List<GameSessionSave> saves, CancellationToken ct);
    }
}