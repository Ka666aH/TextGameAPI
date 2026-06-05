using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISaveService
    {
        Task<Guid> CreateAsync(Guid gameSessionId, SaveType saveType, string? name = null, CancellationToken ct = default);
        Task LoadAsync(Guid gameSessionId, Guid gameSessionSaveId, CancellationToken ct = default);
        Task DeleteAsync(Guid gameSessionId, Guid gameSessionSaveId, CancellationToken ct = default);
        Task<List<GameSessionSave>> GetListAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}