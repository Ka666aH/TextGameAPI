using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IEnemyFactory
    {
        Skeletor CreateSkeletor(IGameSessionService sessionService);
        SkeletorArcher CreateSkeletorArcher(IGameSessionService sessionService);
        Deadman CreateDeadman(IGameSessionService sessionService);
        Ghost CreateGhost(IGameSessionService sessionService);
        Lich CreateLich(IGameSessionService sessionService);
        Mimic CreateMimic(IGameSessionService sessionService);
    }
}