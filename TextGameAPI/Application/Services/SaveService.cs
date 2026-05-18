using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class SaveService : ISaveService
    {
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenRepository _tokenRepository;
        private readonly IGameSessionCacheService _cache;
        private readonly IGameSessionFactory _gameSessionFactory;

        public SaveService(IGameSessionRepository gameSessionRepository, IUnitOfWork unitOfWork, ITokenRepository tokenRepository, IGameSessionCacheService cache, IGameSessionFactory gameSessionFactory)
        {
            _gameSessionRepository = gameSessionRepository;
            _unitOfWork = unitOfWork;
            _tokenRepository = tokenRepository;
            _cache = cache;
            _gameSessionFactory = gameSessionFactory;
        }

        public async Task<Guid> CreateGameSessionAsync(Guid userId, string? gameSessionName, CancellationToken ct = default)
        {
            GameSession gameSession = _gameSessionFactory.CreateGameSession(userId, gameSessionName);
            await _gameSessionRepository.CreateAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cache.SetAsync(gameSession, ct);
            return gameSession.Id;
        }
        public async Task<string> LoadGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();

            await _cache.SetAsync(gameSession, ct);
            return _tokenRepository.GenerateAccessToken(userId, gameSession.Id);
        }
        public async Task SaveGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetWithTrackingAsync(gameSessionId, ct) ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();
            GameSession cachedGameSession = await _cache.GetAsync(gameSessionId, ct) ?? throw new GameSessionNotFoundException();
            if (cachedGameSession.UserId != userId) throw new NotGameSessionOwnerException();

            await _gameSessionRepository.DeleteAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            _unitOfWork.ClearChangeTracker();
            await _gameSessionRepository.CreateAsync(cachedGameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task DeleteGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetWithTrackingAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();

            await _gameSessionRepository.DeleteAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cache.DeleteAsync(gameSessionId, ct);
        }
        public async Task<List<GameSession>> GetGameSessionsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _gameSessionRepository.GetByUserAsync(userId, ct);
        }
    }
}