using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Repositories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class SaveService : ISaveService
    {
        private readonly ISaveRepository _saveRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStateCacheService _cache;
        private readonly ISaveFactory _saveFactory;
        private readonly ISessionAccessGuard _sessionAccessGuard;

        public SaveService(
            ISaveRepository saveRepository, 
            IUnitOfWork unitOfWork, 
            IStateCacheService cache, 
            ISaveFactory saveFactory, 
            ISessionAccessGuard sessionAccessGuard)
        {
            _saveRepository = saveRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _saveFactory = saveFactory;
            _sessionAccessGuard = sessionAccessGuard;
        }

        public async Task<Guid> CreateAsync(Guid gameSessionId, SaveType saveType, string? name = null, CancellationToken ct = default)
        {
            var state =
                saveType != SaveType.Initial ?
                await _cache.GetAsync(gameSessionId, ct) ?? throw new SessionNotFoundException() :
                null;

            var save = saveType switch
            {
                SaveType.Initial => _saveFactory.CreateInitialGameSessionSave(gameSessionId),
                SaveType.Manual => _saveFactory.CreateManualGameSessionSave(gameSessionId, name, state!),
                _ => _saveFactory.CreateAutoGameSessionSave(gameSessionId, state!)
            };

            await _saveRepository.CreateAsync(save, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return save.Id;
        }
        public async Task DeleteAsync(Guid gameSessionId, Guid gameSessionSaveId, CancellationToken ct = default)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(gameSessionId, ct);
            Save gameSessionSave = await _saveRepository.GetAsyncWithTrack(gameSessionSaveId, ct) ?? throw new SaveNotFoundException();
            if (gameSessionSave.Type != SaveType.Manual) throw new ImpossibleDeleteSaveException();
            await _saveRepository.DeleteAsync(gameSessionSave, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<List<Save>> GetListAsync(Guid gameSessionId, CancellationToken ct = default) =>
            await _saveRepository.GetListAsync(gameSessionId, ct);
        public async Task LoadAsync(Guid gameSessionId, Guid gameSessionSaveId, CancellationToken ct = default)
        {
            await _sessionAccessGuard.EnsureOwnershipAsync(gameSessionId, ct);
            Save gameSessionSave = await _saveRepository.GetAsync(gameSessionSaveId, ct) ?? throw new SaveNotFoundException();
            await _cache.SetAsync(gameSessionId, gameSessionSave.State, ct);
        }
    }
}