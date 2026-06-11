using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ISaveService _saveService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionFactory _sessionFactory;
        private readonly IStateCacheService _cache;
        private readonly ITokenRepository _tokenRepository;

        public SessionService(ISessionRepository sessionRepository, ISaveService saveService, IUnitOfWork unitOfWork, ISessionFactory sessionFactory, IStateCacheService cache, ITokenRepository tokenRepository)
        {
            _sessionRepository = sessionRepository;
            _saveService = saveService;
            _unitOfWork = unitOfWork;
            _sessionFactory = sessionFactory;
            _cache = cache;
            _tokenRepository = tokenRepository;
        }

        public async Task<Guid> CreateAsync(Guid userId, string? sessionName, CancellationToken ct = default)
        {
            Session session = _sessionFactory.CreateSession(userId, sessionName);
            await _sessionRepository.CreateAsync(session, ct);

            var newSessionId = session.Id;
            Guid initSaveId = await _saveService.CreateAsync(newSessionId, SaveType.Initial, null, ct);
            await _saveService.LoadAsync(newSessionId, initSaveId, ct);

            return newSessionId;
        }
        public async Task DeleteAsync(Guid sessionId, CancellationToken ct = default)
        {
            Session session = await _sessionRepository.GetAsync(sessionId, ct) ?? throw new SessionNotFoundException();

            await _cache.DeleteAsync(sessionId, ct);
            await _sessionRepository.DeleteAsync(session, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default) =>
            await _sessionRepository.GetListAsync(userId, ct);

        public async Task<string> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct = default) =>
            _tokenRepository.GenerateAccessToken(userId, sessionId);
    }
}