using TextGame.Domain;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Domain;

/// <summary>
/// Weapon behaviour: durability, damage, self harm and the shop multiplier.
/// Swords roll their stats, so deterministic cases use <see cref="ScriptedSword"/>.
/// </summary>
public class WeaponTests
{
    [Fact]
    public void Sword_Attack_DecrementsDurability()
    {
        // Arrange
        var sword = new ScriptedSword(durability: 3, damage: 20);

        // Act
        WeaponAttackResult result = sword.Attack(roomId: 0);

        // Assert
        sword.Durability.Should().Be(2);
        result.IsWeaponBrokenDown.Should().BeFalse();
    }

    [Fact]
    public void Sword_Attack_ReturnsFullDamage()
    {
        // Arrange
        var sword = new ScriptedSword(durability: 3, damage: 20);

        // Act
        WeaponAttackResult result = sword.Attack(roomId: 0);

        // Assert
        result.Should().Be(new WeaponAttackResult(20));
    }

    [Fact]
    public void Sword_Attack_WhenDurabilityReachesZero_ReportsBroken()
    {
        // Arrange
        var sword = new ScriptedSword(durability: 1, damage: 20);

        // Act
        WeaponAttackResult result = sword.Attack(roomId: 0);

        // Assert
        sword.Durability.Should().Be(0);
        result.IsWeaponBrokenDown.Should().BeTrue("the swing that spends the last point of durability breaks the sword");
    }

    [Fact]
    public void Sword_Attack_AfterBreaking_KeepsDealingDamage()
    {
        // Arrange
        var sword = new ScriptedSword(durability: 1, damage: 20);
        sword.Attack(roomId: 0);

        // Act
        WeaponAttackResult result = sword.Attack(roomId: 0);

        // Assert
        result.Damage.Should().Be(20);
        sword.Durability.Should().Be(-1, "nothing removes a broken sword in the domain layer");
    }

    [Fact]
    public void Sword_Attack_IgnoresRoomIdArgument()
    {
        // Arrange
        var sword = new ScriptedSword(durability: 5, damage: 20);

        // Act
        WeaponAttackResult inStartRoom = sword.Attack(roomId: 0);
        WeaponAttackResult inFarRoom = sword.Attack(roomId: 500);

        // Assert
        inFarRoom.Damage.Should().Be(inStartRoom.Damage);
    }

    [Fact]
    public void Sword_Attack_RecalculatesCostFromRemainingDurability()
    {
        // Arrange
        var sword = new ScriptedSword(durability: 3, damage: 20);

        // Act
        sword.Attack(roomId: 0);

        // Assert
        sword.Cost.Should().Be(GameBalance.CalculateSwordCost(durability: 2, damage: 20));
    }

    [Fact]
    public void IronSword_FromRoom_RollsDurabilityWithinSpread()
    {
        // Arrange
        var (expectedMin, expectedMax) = GameBalance.CalculateSpread(GameBalance.IronSwordBaseDurability, 0);

        // Act
        var sword = new IronSword(id: 1, roomId: 0, fromShop: false);

        // Assert
        sword.Durability!.Value.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void IronSword_FromShop_AppliesShopMultiplier()
    {
        // Arrange
        var (min, max) = GameBalance.CalculateSpread(GameBalance.IronSwordBaseDurability, 0);
        int expectedMin = GameBalance.CalculateShopMultiplier(min);
        int expectedMax = GameBalance.CalculateShopMultiplier(max);

        // Act
        var sword = new IronSword(id: 1, roomId: 0, fromShop: true);

        // Assert
        sword.Durability!.Value.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void Fists_Attack_StaysWithinStartRoomSpread()
    {
        // Arrange
        var fists = new Fists();
        var (min, max) = GameBalance.CalculateSpread(GameBalance.FistsBaseDamage, 0);

        // Act + Assert
        for (int attack = 0; attack < 200; attack++)
        {
            WeaponAttackResult result = fists.Attack(roomId: 0);

            result.Damage.Should().BeInRange(min, max);
        }
    }

    [Fact]
    public void Fists_Attack_SelfDamageIsEitherZeroOrHalfTheDamage()
    {
        // Arrange
        var fists = new Fists();

        // Act + Assert
        for (int attack = 0; attack < 200; attack++)
        {
            WeaponAttackResult result = fists.Attack(roomId: 0);

            int expectedSelfDamage = (int)(result.Damage / GameBalance.FistSelfHarmDivider);
            bool noSelfHarm = result.SelfDamage == 0;
            bool halfSelfHarm = result.SelfDamage == expectedSelfDamage;
            (noSelfHarm || halfSelfHarm).Should().BeTrue($"self damage {result.SelfDamage} must be 0 or {expectedSelfDamage}");
        }
    }

    [Fact]
    public void Fists_Attack_GrowsWithRoomId()
    {
        // Arrange
        var fists = new Fists();
        var (min, max) = GameBalance.CalculateSpread(GameBalance.FistsBaseDamage, 50);

        // Act + Assert
        for (int attack = 0; attack < 200; attack++)
        {
            WeaponAttackResult result = fists.Attack(roomId: 50);

            result.Damage.Should().BeInRange(min, max);
        }
    }

    [Fact]
    public void MagicWand_Attack_DealsDamageAndNeverBreaks()
    {
        // Arrange
        var wand = new MagicWand(id: 1, roomId: 0, fromShop: false);
        int damage = wand.Damage;

        // Act + Assert
        for (int attack = 0; attack < 100; attack++)
        {
            WeaponAttackResult result = wand.Attack(roomId: 0);

            result.Should().Be(new WeaponAttackResult(damage));
            wand.Durability.Should().BeNull("wands have no durability");
        }
    }

    [Fact]
    public void MagicWand_CostIsTwiceItsDamage()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var wand = new MagicWand(id: 1, roomId: 0, fromShop: false);

        // Assert
        wand.Cost.Should().Be(GameBalance.CalculateWandCost(wand.Damage));
    }

    [Fact]
    public void RandomWand_Attack_ReturnsZeroUpToItsDamage()
    {
        // Arrange
        var wand = new RandomWand(id: 1, roomId: 0, fromShop: false);

        // Act + Assert
        for (int attack = 0; attack < 200; attack++)
        {
            WeaponAttackResult result = wand.Attack(roomId: 0);

            result.Damage.Should().BeInRange(0, wand.Damage);
        }
    }

    [Fact]
    public void RandomWand_Attack_IgnoresRoomIdArgument()
    {
        // Arrange
        var wand = new RandomWand(id: 1, roomId: 0, fromShop: false);

        // Act + Assert
        for (int attack = 0; attack < 100; attack++)
        {
            WeaponAttackResult result = wand.Attack(roomId: 1000);

            result.Damage.Should().BeInRange(0, wand.Damage, "the wand scales by the room it was created in, not by the argument");
        }
    }

    [Fact]
    public void GlassSword_AlwaysStartsWithOnePointOfDurability()
    {
        // Arrange
        const int attempts = 100;

        // Act + Assert
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            var sword = new GlassSword(id: 1, roomId: 0, fromShop: false);

            sword.Durability!.Value.Should().Be(1, "a weapon must never be born broken");
        }
    }
}
