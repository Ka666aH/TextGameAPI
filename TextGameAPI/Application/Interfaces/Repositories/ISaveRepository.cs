using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Repositories
{
    public interface ISaveRepository
    {
        Task CreateAsync(Save save, CancellationToken ct = default);
        Task<Save?> GetAsyncWithTrack(Guid saveId, CancellationToken ct = default);
        Task<Save?> GetAsync(Guid saveId, CancellationToken ct = default);
        Task<Save?> GetLastSaveAsync(Guid sessionId, CancellationToken ct = default);
        Task<List<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default);
        Task DeleteAsync(Save save, CancellationToken ct = default);

        Task<Dictionary<Guid, string?>> GetAutoSaveHashesAsync(List<Guid> sessionIds, CancellationToken ct);
        Task BatchReplaceAutoSavesAsync(List<Guid> sessionIds, List<Save> saves, CancellationToken ct);
    }
}