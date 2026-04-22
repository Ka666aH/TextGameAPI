using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Rooms;

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
            Room room = _gameSessionService.CurrentRoom!;
            Enemy? enemy = room.Enemies.FirstOrDefault();
            return enemy ?? throw new NullEnemyIdException();
        }
    }
}