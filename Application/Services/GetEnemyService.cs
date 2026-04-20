using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GetEnemyService : IGetEnemyService
    {
        private readonly IGameSessionService _gameSessionService;
        public GetEnemyService(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }
        public Enemy GetEnemy()
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();

            Room room = _gameSessionService.CurrentRoom!;
            Enemy? enemy = room.Enemies.FirstOrDefault();
            if (enemy == null) throw new NullEnemyIdException();
            return enemy;
        }
    }
}