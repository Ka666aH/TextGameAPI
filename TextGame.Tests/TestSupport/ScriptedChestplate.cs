using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Chestplate with forced durability and damage block.
/// </summary>
internal sealed class ScriptedChestplate : Chestplate
{
    public ScriptedChestplate(int durability, int damageBlock)
        : base(0, "Test Chestplate", "Deterministic chestplate for tests", 0, false, 1, 1)
    {
        Durability = durability;
        DamageBlock = damageBlock;
    }
}
