using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Enemy with fixed stats: production enemies roll their stats through Random.Shared,
/// this double skips the roll so combat tests stay deterministic.
/// </summary>
internal sealed class TestEnemy : Enemy
{
    public TestEnemy(int health = 100, int damage = 10, int damageBlock = 0, int roomId = 0)
        : base(0, "Test Enemy", "Deterministic enemy for tests", roomId, health, damage, damageBlock)
    {
    }

    public override void Initialize(int health, int damage, int damageBlock, Random? random = null)
    {
        Health = health;
        Damage = damage;
        DamageBlock = damageBlock;
    }
}
