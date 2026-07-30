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

        public SaveService(
            ISaveRepository saveRepository,
            IUnitOfWork unitOfWork,
            IStateCacheService cache,
            ISaveFactory saveFactory)
        {
            _saveRepository = saveRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _saveFactory = saveFactory;
        }

        public async Task<Guid> CreateAsync(Guid sessionId, SaveType saveType, string? name = null, CancellationToken ct = default)
        {
            var state =
                saveType != SaveType.Initial ?
                await _cache.GetAsync(sessionId, ct) ?? throw new SessionNotFoundException() :
                null;

            var save = saveType switch
            {
                SaveType.Initial => _saveFactory.CreateInitial(sessionId),
                SaveType.Manual => _saveFactory.CreateManual(sessionId, name, state!),
                _ => _saveFactory.CreateAuto(sessionId, state!)
            };

            await _saveRepository.CreateAsync(save, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return save.Id;
        }
        public async Task DeleteAsync(Guid sessionId, Guid saveId, CancellationToken ct = default)
        {
            Save save = await _saveRepository.GetAsyncWithTrack(saveId, ct) ?? throw new SaveNotFoundException();
            if (save.SessionId != sessionId) throw new NotSaveOwnerException();
            if (save.Type != SaveType.Manual) throw new ImpossibleDeleteSaveException();
            await _saveRepository.DeleteAsync(save, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyList<Save>> GetListAsync(Guid sessionId, CancellationToken ct = default) =>
            await _saveRepository.GetListAsync(sessionId, ct);
        public async Task LoadAsync(Guid sessionId, Guid saveId, CancellationToken ct = default)
        {
            Save save = await _saveRepository.GetAsync(saveId, ct) ?? throw new SaveNotFoundException();
            if (save.SessionId != sessionId) throw new NotSaveOwnerException();
            await _cache.SetAsync(sessionId, save.State, ct);
        }
    }
}