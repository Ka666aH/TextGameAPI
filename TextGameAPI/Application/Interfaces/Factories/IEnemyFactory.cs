using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Application.Interfaces.Factories
{
    public interface IEnemyFactory
    {
        Skeletor CreateSkeletor(Random random);
        SkeletorArcher CreateSkeletorArcher(Random random);
        Deadman CreateDeadman(Random random);
        Ghost CreateGhost(Random random);
        Lich CreateLich(Random random);
        Mimic CreateMimic(Random random);
    }
}
