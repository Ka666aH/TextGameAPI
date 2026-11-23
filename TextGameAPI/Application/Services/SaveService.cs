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
        private readonly ISessionProvider _sessionProvider;

        public SaveService(
            ISaveRepository saveRepository,
            IUnitOfWork unitOfWork,
            IStateCacheService cache,
            ISaveFactory saveFactory,
            ISessionProvider sessionProvider)
        {
            _saveRepository = saveRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _saveFactory = saveFactory;
            _sessionProvider = sessionProvider;
        }

        public async Task<Guid> CreateAsync(Guid sessionId, SaveType saveType, string? name = null, long? seed = null, CancellationToken ct = default)
        {
            // Ручное сохранение читает состояние через провайдер: если снапшот вытеснен из Redis,
            // он поднимется из последнего сейва, а не упадёт SESSION_NOT_FOUND (п. 8).
            var state = saveType switch
            {
                SaveType.Initial => null,
                SaveType.Manual => await _sessionProvider.GetAsync(sessionId, ct),
                _ => await _cache.GetAsync(sessionId, ct) ?? throw new SessionNotFoundException()
            };

            var save = saveType switch
            {
                SaveType.Initial => _saveFactory.CreateInitial(sessionId, seed ?? Random.Shared.NextInt64()),
                SaveType.Manual => _saveFactory.CreateManual(sessionId, name, state!),
                _ => _saveFactory.CreateAuto(sessionId, state!)
            };

            await _saveRepository.CreateAsync(save, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return save.Id;
        }
        public async Task SaveAutoAsync(Guid sessionId, CancellationToken ct = default)
        {
            // Немедленный автосейв на конец игры: заменяем существующую строку, а не создаём
            // новую — так же, как фоновый AutoSaveService (notes/fixes-1.2.0.md, п. 11).
            var state = await _cache.GetAsync(sessionId, ct);
            if (state is null) return;

            var save = _saveFactory.CreateAuto(sessionId, state);
            await _saveRepository.BatchReplaceAutoSavesAsync([sessionId], [save], ct);
            await _unitOfWork.SaveChangesAsync(ct);
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