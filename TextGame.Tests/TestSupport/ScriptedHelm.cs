using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Helm with durability and damage block forced after construction, because production
/// armor rolls both values through Random.Shared.
/// </summary>
internal sealed class ScriptedHelm : Helm
{
    public ScriptedHelm(int durability, int damageBlock, int roomId = 0)
        : base(0, "Test Helm", "Deterministic helm for tests", roomId, false, 1, 1)
    {
        Durability = durability;
        DamageBlock = damageBlock;
    }
}
