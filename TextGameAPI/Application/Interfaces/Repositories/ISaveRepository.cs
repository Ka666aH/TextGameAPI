using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface ISaveRepository
    {
        Task CreateAsync(Save gameSessionSave, CancellationToken ct = default);
        Task<Save?> GetAsyncWithTrack(Guid gameSessionSaveId, CancellationToken ct = default);
        Task<Save?> GetAsync(Guid gameSessionSaveId, CancellationToken ct = default);
        Task<Save?> GetLastSaveAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<List<Save>> GetListAsync(Guid gameSessionId, CancellationToken ct = default);
        Task DeleteAsync(Save gameSessionSave, CancellationToken ct = default);

        Task<Dictionary<Guid, string?>> GetAutoSaveHashesAsync(List<Guid> sessionIds, CancellationToken ct);
        Task BatchReplaceAutoSavesAsync(List<Guid> sessionIds, List<Save> saves, CancellationToken ct);
    }
}