using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class SaveService : ISaveService
    {
        private readonly IGameSessionSaveRepository _gameSessionSaveRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameSessionStateCacheService _cache;
        private readonly IGameSessionSaveFactory _gameSessionSaveFactory;
        private readonly ISessionAccessGuard _sessionAccessGuard;

        public SaveService(IGameSessionSaveRepository gameSessionSaveRepository, IUnitOfWork unitOfWork, IGameSessionStateCacheService cache, IGameSessionSaveFactory gameSessionSaveFactory, ISessionAccessGuard sessionAccessGuard)
        {
            _gameSessionSaveRepository = gameSessionSaveRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _gameSessionSaveFactory = gameSessionSaveFactory;
            _sessionAccessGuard = sessionAccessGuard;
        }

        public async Task<Guid> CreateAsync(Guid gameSessionId, SaveType saveType, string? name = null, CancellationToken ct = default)
        {
            var state =
                saveType != SaveType.Initial ?
                await _cache.GetAsync(gameSessionId, ct) ?? throw new GameSessionNotFoundException() :
                null;

            var save = saveType switch
            {
                SaveType.Initial => _gameSessionSaveFactory.CreateInitialGameSessionSave(gameSessionId),
                SaveType.Manual => _gameSessionSaveFactory.CreateManualGameSessionSave(gameSessionId, name, state!),
                _ => _gameSessionSaveFactory.CreateAutoGameSessionSave(gameSessionId, state!)
            };

            await _gameSessionSaveRepository.CreateAsync(save, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return save.Id;
        }
        public async Task DeleteAsync(Guid gameSessionId, Guid gameSessionSaveId, CancellationToken ct = default)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(gameSessionId, ct);
            GameSessionSave gameSessionSave = await _gameSessionSaveRepository.GetAsyncWithTrack(gameSessionSaveId, ct) ?? throw new GameSessionSaveNotFoundException();
            if (gameSessionSave.Type != SaveType.Manual) throw new ImpossibleDeleteSaveException();
            await _gameSessionSaveRepository.DeleteAsync(gameSessionSave, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<List<GameSessionSave>> GetListAsync(Guid gameSessionId, CancellationToken ct = default) =>
            await _gameSessionSaveRepository.GetListAsync(gameSessionId, ct);
        public async Task LoadAsync(Guid gameSessionId, Guid gameSessionSaveId, CancellationToken ct = default)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(gameSessionId, ct);
            GameSessionSave gameSessionSave = await _gameSessionSaveRepository.GetAsync(gameSessionSaveId, ct) ?? throw new GameSessionSaveNotFoundException();
            await _cache.SetAsync(gameSessionId, gameSessionSave.State, ct);
        }
    }
}