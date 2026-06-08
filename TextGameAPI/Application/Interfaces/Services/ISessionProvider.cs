using TextGame.Domain.Entities;

namespace TextGame.Application.Interfaces.Services
{
    public interface ISessionProvider
    {
        Task<State> GetAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}
