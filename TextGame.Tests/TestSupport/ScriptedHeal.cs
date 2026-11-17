using TextGame.Domain.Entities.GameObjects.Items.Heals;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// Potion with fixed boosts: production potions roll through Random.Shared, and
/// RandomPotion can heal or poison depending on the roll.
/// </summary>
internal sealed class ScriptedHeal : Heal
{
    private readonly int _maxHealthBoost;
    private readonly int _currentHealthBoost;

    public ScriptedHeal(int maxHealthBoost, int currentHealthBoost)
        : base(0, "Test Potion", "Deterministic potion for tests", 0, false, 0, 0)
    {
        _maxHealthBoost = maxHealthBoost;
        _currentHealthBoost = currentHealthBoost;
    }

    public override (int, int) Use() => (_maxHealthBoost, _currentHealthBoost);
}
