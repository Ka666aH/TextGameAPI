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

        public async Task<Session> CreateAsync(Guid userId, string? sessionName, long? seed = null, CancellationToken ct = default)
        {
            Session session = _sessionFactory.CreateSession(userId, sessionName, seed);
            await _sessionRepository.CreateAsync(session, ct);

            Guid initSaveId = await _saveService.CreateAsync(session.Id, SaveType.Initial, seed: session.Seed, ct: ct);
            await _saveService.LoadAsync(session.Id, initSaveId, ct);

            return session;
        }
        public async Task DeleteAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
        {
            Session session = await _sessionRepository.GetAsync(sessionId, ct) ?? throw new SessionNotFoundException();
            // Проверка владельца здесь, а не только в фильтре: удалять сессию можно и без активной игры,
            // а фильтр RequireSessionOwnership требует claim SessionId и такую возможность отрезал.
            if (session.UserId != userId) throw new NotSessionOwnerException();

            await _cache.DeleteAsync(sessionId, ct);
            await _sessionRepository.DeleteAsync(session, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyList<Session>> GetListAsync(Guid userId, CancellationToken ct = default) =>
            await _sessionRepository.GetListAsync(userId, ct);

        public async Task<string> LoadAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
        {
            Session session = await _sessionRepository.GetAsync(sessionId, ct)
                ?? throw new SessionNotFoundException();
            if (session.UserId != userId) throw new NotSessionOwnerException();

            return _tokenRepository.GenerateAccessToken(userId, sessionId);
        }

        public string CreateAccessToken(Guid userId) => _tokenRepository.GenerateAccessToken(userId);
    }
}