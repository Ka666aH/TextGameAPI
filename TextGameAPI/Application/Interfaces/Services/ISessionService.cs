using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISessionService
    {
        Task<Session> CreateAsync(Guid userId, string? sessionName, long? seed = null, CancellationToken ct = default);
        Task<string> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
        Task DeleteAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
        /// <summary>Access-токен без привязки к игре — после удаления активной сессии.</summary>
        string CreateAccessToken(Guid userId);
        Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default);
    }
}