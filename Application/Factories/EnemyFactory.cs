using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Factories
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyIdService _enemyIdService;

        private readonly int _roomId;

        public EnemyFactory(IEnemyIdService enemyIdService, IRoomIdService roomIdService)
        {
            _enemyIdService = enemyIdService;

            _roomId = roomIdService.Current();
        }

        public Skeletor CreateSkeletor() => new Skeletor(_roomId, _enemyIdService.Next());
        public SkeletorArcher CreateSkeletorArcher() => new SkeletorArcher(_roomId, _enemyIdService.Next());
        public Deadman CreateDeadman() => new Deadman(_roomId, _enemyIdService.Next());
        public Ghost CreateGhost() => new Ghost(_roomId, _enemyIdService.Next());
        public Lich CreateLich() => new Lich(_roomId, _enemyIdService.Next());
        public Mimic CreateMimic() => new Mimic(_roomId, _enemyIdService.Next());
    }
}