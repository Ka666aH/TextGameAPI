using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Application.Factories
{
    public class EnemyFactory : IEnemyFactory
    {
        public Skeletor CreateSkeletor(IGameSessionService sessionService) => new Skeletor(sessionService.RoomCounter, sessionService.NextEnemyId());
        public SkeletorArcher CreateSkeletorArcher(IGameSessionService sessionService) => new SkeletorArcher(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Deadman CreateDeadman(IGameSessionService sessionService) => new Deadman(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Ghost CreateGhost(IGameSessionService sessionService) => new Ghost(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Lich CreateLich(IGameSessionService sessionService) => new Lich(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Mimic CreateMimic(IGameSessionService sessionService) => new Mimic(sessionService.RoomCounter, sessionService.NextEnemyId());
    }
}