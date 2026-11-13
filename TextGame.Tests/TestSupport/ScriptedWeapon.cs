using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Weapon that returns a fixed attack result: no durability roll, no self-harm roll.
/// </summary>
internal sealed class ScriptedWeapon : Weapon
{
    private readonly WeaponAttackResult _result;

    public ScriptedWeapon(int damage, int selfDamage = 0, bool isWeaponBrokenDown = false)
        : base(0, "Test Weapon", "Deterministic weapon for tests", null, 0, 0, false)
    {
        _result = new WeaponAttackResult(damage, selfDamage, isWeaponBrokenDown);
    }

    public override WeaponAttackResult Attack(int roomId) => _result;
}
