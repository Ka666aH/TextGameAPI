using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Presentation.Helpers;

namespace TextGame.Application.Services
{
    public class SessionAccessGuard : ISessionAccessGuard
    {
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionAccessGuard(IGameSessionRepository gameSessionRepository, IHttpContextAccessor httpContextAccessor)
        {
            _gameSessionRepository = gameSessionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task EnsureOwnershipAsync(Guid gameSessionId, CancellationToken ct)
        {
            await GetOwnedSession(gameSessionId, ct);
        }

        public async Task<GameSession> GetOwnedSession(Guid gameSessionId, CancellationToken ct)
        {
            if (!_httpContextAccessor.HttpContext!.User.TryGetUserId(out Guid userId)) throw new MissingUserIdClaimException();

            GameSession gameSession = await _gameSessionRepository.GetAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();
            return gameSession;
        }
    }
}