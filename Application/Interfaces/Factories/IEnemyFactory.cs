using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IEnemyFactory
    {
        Skeletor CreateSkeletor();
        SkeletorArcher CreateSkeletorArcher();
        Deadman CreateDeadman();
        Ghost CreateGhost();
        Lich CreateLich();
        Mimic CreateMimic();
    }
}