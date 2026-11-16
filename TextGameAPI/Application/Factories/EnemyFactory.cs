using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Application.Factories
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyIdService _enemyIdService;
        private readonly IRoomIdService _roomIdService;

        public EnemyFactory(IEnemyIdService enemyIdService, IRoomIdService roomIdService)
        {
            _enemyIdService = enemyIdService;
            _roomIdService = roomIdService;
        }

        public Skeletor CreateSkeletor(Random random) => new(_enemyIdService.Next(), _roomIdService.Current(), random);
        public SkeletorArcher CreateSkeletorArcher(Random random) => new(_enemyIdService.Next(), _roomIdService.Current(), random);
        public Deadman CreateDeadman(Random random) => new(_enemyIdService.Next(), _roomIdService.Current(), random);
        public Ghost CreateGhost(Random random) => new(_enemyIdService.Next(), _roomIdService.Current(), random);
        public Lich CreateLich(Random random) => new(_enemyIdService.Next(), _roomIdService.Current(), random);
        public Mimic CreateMimic(Random random) => new(_enemyIdService.Next(), _roomIdService.Current(), random);
    }
}
