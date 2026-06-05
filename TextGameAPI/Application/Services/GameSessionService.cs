using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;

namespace TextGame.Application.Services
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameSessionFactory _gameSessionFactory;
        private readonly IGameSessionStateCacheService _cache;
        private readonly ITokenRepository _tokenRepository;
        private readonly ISessionAccessGuard _sessionAccessGuard;

        public GameSessionService(IGameSessionRepository gameSessionRepository, IUnitOfWork unitOfWork, IGameSessionFactory gameSessionFactory, IGameSessionStateCacheService cache, ITokenRepository tokenRepository, ISessionAccessGuard sessionAccessGuard)
        {
            _gameSessionRepository = gameSessionRepository;
            _unitOfWork = unitOfWork;
            _gameSessionFactory = gameSessionFactory;
            _cache = cache;
            _tokenRepository = tokenRepository;
            _sessionAccessGuard = sessionAccessGuard;
        }
        public async Task<Guid> CreateAsync(Guid userId, string? gameSessionName, CancellationToken ct = default)
        {
            GameSession gameSession = _gameSessionFactory.CreateGameSession(userId, gameSessionName);
            await _gameSessionRepository.CreateAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return gameSession.Id;
        }
        public async Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _sessionAccessGuard.GetOwnedSession(gameSessionId, ct);

            await _cache.DeleteAsync(gameSessionId, ct);
            await _gameSessionRepository.DeleteAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<List<GameSession>> GetListAsync(Guid userId, CancellationToken ct = default) =>
            await _gameSessionRepository.GetListAsync(userId, ct);

        public async Task<string> LoadAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(gameSessionId, ct);
            return _tokenRepository.GenerateAccessToken(userId, gameSessionId);
        }
    }
}