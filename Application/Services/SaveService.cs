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
        //private readonly ICacheSerivce _cache;
        //here

        public SaveService(IGameSessionRepository gameSessionRepository, IUnitOfWork unitOfWork, ITokenRepository tokenRepository)
        {
            _gameSessionRepository = gameSessionRepository;
            _unitOfWork = unitOfWork;
            _tokenRepository = tokenRepository;
        }

        public async Task<string> NewGameSessionAsync(Guid userId, CancellationToken ct = default)
        {
            GameSession gameSession = new(userId);
            await _gameSessionRepository.CreateAsync(gameSession, ct);
            //добавить сессию в кэш
            await _unitOfWork.SaveChangesAsync(ct);
            return _tokenRepository.GenerateAccessToken(userId, gameSession.Id);
        }
        public async Task<string> LoadGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();

            //добавить сессию в кэш
            return _tokenRepository.GenerateAccessToken(userId, gameSession.Id);
        }
        public async Task SaveGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            //GameSession gameSession = await _gameSessionRepository.GetWithTrackingAsync(gameSessionId, ct)
            //    ?? throw new GameSessionNotFoundException();
            //if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();
            //достать сессию из кэша?
            //обновить в БД
            //gameSession = сессия из кэша;
            //await _unitOfWork.SaveChangesAsync(ct);
            throw new NotImplementedException();
        }
        public async Task DeleteGameSessionAsync(Guid userId, Guid gameSessionId, CancellationToken ct = default)
        {
            GameSession gameSession = await _gameSessionRepository.GetWithTrackingAsync(gameSessionId, ct)
                ?? throw new GameSessionNotFoundException();
            if (gameSession.UserId != userId) throw new NotGameSessionOwnerException();

            await _gameSessionRepository.DeleteAsync(gameSession, ct);
            //удалить сессию из кэша
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<List<GameSession>> GetGameSessions(Guid userId, CancellationToken ct = default)
        {
            return await _gameSessionRepository.GetByUserAsync(userId, ct);
        }
    }
}