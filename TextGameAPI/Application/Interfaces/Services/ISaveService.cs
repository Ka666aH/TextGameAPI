using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISaveService
    {
        Task<Guid> CreateAsync(Guid sessionId, SaveType saveType, string? name = null, CancellationToken ct = default);
        Task LoadAsync(Guid sessionId, Guid saveId, CancellationToken ct = default);
        Task DeleteAsync(Guid saveId, CancellationToken ct = default);
        Task<List<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default);
    }
}