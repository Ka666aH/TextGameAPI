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
        private readonly IGameSessionStateCacheService _cache;
        private readonly IGameSessionFactory _gameSessionFactory;

        public SaveService(IGameSessionRepository gameSessionRepository, IUnitOfWork unitOfWork, ITokenRepository tokenRepository, IGameSessionStateCacheService cache, IGameSessionFactory gameSessionFactory)
        {
            _gameSessionRepository = gameSessionRepository;
            _unitOfWork = unitOfWork;
            _tokenRepository = tokenRepository;
            _cache = cache;
            _gameSessionFactory = gameSessionFactory;
        }

        public async Task<Guid> CreateAsync(Guid userId, string? gameSessionName, CancellationToken ct = default)
        {
            GameSession gameSession = _gameSessionFactory.CreateGameSession(userId, gameSessionName);
            await _gameSessionRepository.CreateAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return gameSession.Id;
        }
        public async Task<string> LoadAsync(Guid userId, Guid currentGameSessionId, Guid loadingGameSessionId, CancellationToken ct = default)
        {
            GameSessionState gameSessionState = await _gameSessionRepository.GetStateAsync(loadingGameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (currentGameSessionId == Guid.Empty) await _cache.DeleteAsync(currentGameSessionId, ct);
            await _cache.SetAsync(loadingGameSessionId, gameSessionState, ct);
            return _tokenRepository.GenerateAccessToken(userId, loadingGameSessionId);
        }
        public async Task SaveAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();
            GameSessionState cachedGameSessionState = await _cache.GetAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            gameSession.SetState(cachedGameSessionState);
            gameSession.UpdateLastSavedAt();
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task DeleteAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();

            await _gameSessionRepository.DeleteAsync(gameSession, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _cache.DeleteAsync(gameSessionId, ct);
        }
        public async Task<List<GameSession>> GetListAsync(Guid userId, CancellationToken ct = default)
        {
            return await _gameSessionRepository.GetListAsync(userId, ct);
        }
    }
}