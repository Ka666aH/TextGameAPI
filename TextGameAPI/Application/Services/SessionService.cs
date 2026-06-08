using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;

namespace TextGame.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionFactory _sessionFactory;
        private readonly IStateCacheService _cache;
        private readonly ITokenRepository _tokenRepository;
        private readonly ISessionAccessGuard _sessionAccessGuard;

        public SessionService(
            ISessionRepository sessionRepository, 
            IUnitOfWork unitOfWork, 
            ISessionFactory sessionFactory, 
            IStateCacheService cache, 
            ITokenRepository tokenRepository, 
            ISessionAccessGuard sessionAccessGuard)
        {
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
            _sessionFactory = sessionFactory;
            _cache = cache;
            _tokenRepository = tokenRepository;
            _sessionAccessGuard = sessionAccessGuard;
        }
        public async Task<Guid> CreateAsync(Guid userId, string? gameSessionName, CancellationToken ct = default)
        {
            Session gameSession = _sessionFactory.CreateGameSession(userId, gameSessionName);
            await _sessionRepository.CreateAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return gameSession.Id;
        }
        public async Task DeleteAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            Session gameSession = await _sessionAccessGuard.GetOwnedSession(gameSessionId, ct);

            await _cache.DeleteAsync(gameSessionId, ct);
            await _sessionRepository.DeleteAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<List<Session>> GetListAsync(Guid userId, CancellationToken ct = default) =>
            await _sessionRepository.GetListAsync(userId, ct);

        public async Task<string> LoadAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(gameSessionId, ct);
            return _tokenRepository.GenerateAccessToken(userId, gameSessionId);
        }
    }
}