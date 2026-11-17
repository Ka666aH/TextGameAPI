using TextGame.Domain;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;

namespace TextGame.Tests.Domain;

/// <summary>
/// Defaults of a fresh game state. These pin what the API hands out on a new session.
/// </summary>
public class StateTests
{
    [Fact]
    public void NewState_StartsGameWithFullHealth()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var state = new State();

        // Assert
        state.IsGameStarted.Should().BeTrue();
        state.IsInBattle.Should().BeFalse();
        state.MaxHealth.Should().Be(GameBalance.DefaultMaxHealth);
        state.CurrentHealth.Should().Be(GameBalance.DefaultMaxHealth);
    }

    [Fact]
    public void NewState_StartsWithNoMoneyNoKeysAndEmptyBags()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var state = new State();

        // Assert
        state.Coins.Should().Be(GameBalance.StartCoins);
        state.Keys.Should().Be(GameBalance.StartKeys);
        state.Inventory.Should().BeEmpty();
        state.Rooms.Should().BeEmpty();
        state.CurrentRoomId.Should().Be(0);
        state.CurrentMimicChest.Should().BeNull();
    }

    [Fact]
    public void NewState_StartsBareHanded()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var state = new State();

        // Assert
        state.Weapon.Should().BeOfType<Fists>();
        state.Weapon.Damage.Should().Be(GameBalance.FistsBaseDamage);
        state.Helm.Should().BeNull();
        state.Chestplate.Should().BeNull();
    }

    [Fact]
    public void NewState_SharesOneStaticFistsInstance()
    {
        // Arrange
        var firstState = new State();

        // Act
        var secondState = new State();

        // Assert
        secondState.Weapon.Should().BeSameAs(firstState.Weapon,
            "StartWeapon is a static readonly field, so every state points at the same Fists instance");
        firstState.Weapon.Should().BeSameAs(GameBalance.StartWeapon);
    }
}
