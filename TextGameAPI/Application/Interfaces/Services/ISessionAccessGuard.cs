using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISessionAccessGuard
    {
        Task<GameSession> GetOwnedSession(Guid gameSessionId, CancellationToken ct);
        Task EnsureOwnershipAsync(Guid gameSessionId, CancellationToken ct);
    }
}
