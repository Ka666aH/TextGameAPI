using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISessionAccessGuard
    {
        Task EnsureOwnershipAsync(Guid sessionId, CancellationToken ct);
    }
}
