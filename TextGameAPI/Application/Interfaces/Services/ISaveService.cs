using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISaveService
    {
        Task<Guid> CreateAsync(Guid sessionId, SaveType saveType, string? name = null, CancellationToken ct = default);
        Task LoadAsync(Guid sessionId, Guid saveId, CancellationToken ct = default);
        Task DeleteAsync(Guid sessionId, Guid saveId, CancellationToken ct = default);
        Task<IReadOnlyList<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default);
    }
}