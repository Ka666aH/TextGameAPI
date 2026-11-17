using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Sword with durability and damage forced after construction, because production
/// swords roll both values through Random.Shared.
/// </summary>
internal sealed class ScriptedSword : Sword
{
    public ScriptedSword(int durability, int damage, int roomId = 0)
        : base(0, "Test Sword", "Deterministic sword for tests", roomId, false, 1, 1)
    {
        Durability = durability;
        Damage = damage;
    }
}
