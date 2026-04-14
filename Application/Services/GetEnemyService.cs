using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class GetEnemyService : IGetEnemyService
    {
        private readonly IGameSessionService _sessionService;
        public GetEnemyService(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public Enemy GetEnemyById()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            Room room = _sessionService.CurrentRoom!;
            Enemy? enemy = room.Enemies.FirstOrDefault();
            if (enemy == null) throw new NullEnemyIdException();
            return enemy;
        }
    }
}