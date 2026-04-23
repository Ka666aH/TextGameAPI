using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Application.Factories
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyIdService _enemyIdService;
        private readonly IRoomIdService _roomIdService;

        public EnemyFactory(IEnemyIdService enemyIdService, IRoomIdService roomIdService, IRoomIdService rooomIdService)
        {
            _enemyIdService = enemyIdService;
            _roomIdService = rooomIdService;
        }

        public Skeletor CreateSkeletor() => new(_enemyIdService.Next(), _roomIdService.Current());
        public SkeletorArcher CreateSkeletorArcher() => new(_enemyIdService.Next(), _roomIdService.Current());
        public Deadman CreateDeadman() => new(_enemyIdService.Next(), _roomIdService.Current());
        public Ghost CreateGhost() => new(_enemyIdService.Next(), _roomIdService.Current());
        public Lich CreateLich() => new(_enemyIdService.Next(), _roomIdService.Current());
        public Mimic CreateMimic() => new(_enemyIdService.Next(), _roomIdService.Current());
    }
}