using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Application.Services
{
    public class GetEnemyService : IGetEnemyService
    {
        private readonly IGameSessionStateService _gameSessionService;
        public GetEnemyService(IGameSessionStateService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }
        public Enemy GetEnemy() =>
            _gameSessionService.CurrentRoom.Enemy ?? throw new NullEnemyIdException();
    }
}