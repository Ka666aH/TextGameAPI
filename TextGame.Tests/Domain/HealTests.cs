using TextGame.Domain;
using TextGame.Domain.Entities.GameObjects.Items.Heals;

namespace TextGame.Tests.Domain;

/// <summary>
/// Potions and bandages: rolled boosts, healing cost and the random potion's swing
/// between healing and poisoning.
/// </summary>
public class HealTests
{
    [Fact]
    public void Bandage_RollsOnlyCurrentHealthBoost()
    {
        // Arrange
        var (expectedMin, expectedMax) = GameBalance.CalculateSpread(GameBalance.BandageBaseCurrentHealthBoost, 0);

        // Act
        var bandage = new Bandage(id: 1, roomId: 0, fromShop: false);

        // Assert
        bandage.MaxHealthBoost.Should().Be(0);
        bandage.CurrentHealthBoost!.Value.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void Bandage_Use_ReturnsTheRolledBoosts()
    {
        // Arrange
        var bandage = new Bandage(id: 1, roomId: 0, fromShop: false);
        int expectedMax = bandage.MaxHealthBoost!.Value;
        int expectedCurrent = bandage.CurrentHealthBoost!.Value;

        // Act
        var (maxHealthBoost, currentHealthBoost) = bandage.Use();

        // Assert
        maxHealthBoost.Should().Be(expectedMax);
        currentHealthBoost.Should().Be(expectedCurrent);
    }

    [Fact]
    public void Bandage_CostIsDerivedFromItsBoosts()
    {
        // Arrange
        var bandage = new Bandage(id: 1, roomId: 0, fromShop: false);

        // Act
        int cost = bandage.Cost!.Value;

        // Assert
        cost.Should().Be(GameBalance.CalculateHealCost(bandage.MaxHealthBoost, bandage.CurrentHealthBoost));
    }

    [Fact]
    public void PowerPotion_RollsBothBoosts()
    {
        // Arrange
        var (minMax, maxMax) = GameBalance.CalculateSpread(GameBalance.PowerPotionBaseMaxHealthBoost, 0);
        var (minCurrent, maxCurrent) = GameBalance.CalculateSpread(GameBalance.PowerPotionBaseCurrentHealthBoost, 0);

        // Act
        var potion = new PowerPotion(id: 1, roomId: 0, fromShop: false);

        // Assert
        potion.MaxHealthBoost!.Value.Should().BeInRange(minMax, maxMax);
        potion.CurrentHealthBoost!.Value.Should().BeInRange(minCurrent, maxCurrent);
    }

    [Fact]
    public void RegenPotion_HealsCurrentHealthOnly()
    {
        // Arrange
        var (expectedMin, expectedMax) = GameBalance.CalculateSpread(GameBalance.RegenPotionBaseCurrentHealthBoost, 0);

        // Act
        var potion = new RegenPotion(id: 1, roomId: 0, fromShop: false);

        // Assert
        potion.MaxHealthBoost.Should().Be(0);
        potion.CurrentHealthBoost!.Value.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void Bandage_FromShop_MultipliesItsBoosts()
    {
        // Arrange
        var (minCurrent, maxCurrent) = GameBalance.CalculateSpread(GameBalance.BandageBaseCurrentHealthBoost, 0);
        int expectedMin = GameBalance.CalculateShopMultiplier(minCurrent);
        int expectedMax = GameBalance.CalculateShopMultiplier(maxCurrent);

        // Act
        var bandage = new Bandage(id: 1, roomId: 0, fromShop: true);

        // Assert
        bandage.MaxHealthBoost.Should().Be(0);
        bandage.CurrentHealthBoost!.Value.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void RandomPotion_StartsWithoutFixedBoosts()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var potion = new RandomPotion(id: 1, roomId: 0, fromShop: false);

        // Assert
        potion.MaxHealthBoost.Should().BeNull("the boosts are rolled only when the potion is used");
        potion.CurrentHealthBoost.Should().BeNull();
    }

    [Fact]
    public void RandomPotion_CostStaysInsideSpreadAndBelowExclusiveCeiling()
    {
        // Arrange
        var (min, max) = GameBalance.CalculateSpread(GameBalance.CalculateRandomPotionBaseCost(), 0);

        // Act
        var potion = new RandomPotion(id: 1, roomId: 0, fromShop: false);

        // Assert
        potion.Cost!.Value.Should().BeInRange(min, max - 1, "Random.Next uses an exclusive upper bound, so the top of the spread is never reachable");
    }

    [Fact]
    public void RandomPotion_Use_CanPoisonThePlayer()
    {
        // Arrange
        const int roomId = 0;
        int maxBase = (int)(GameBalance.RandomPotionBaseMaxHealthBoost * GameBalance.CalculateGain(roomId));
        int currentBase = (int)(GameBalance.RandomPotionBaseCurrentHealthBoost * GameBalance.CalculateGain(roomId));
        const int attempts = 200;

        // Act + Assert
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            var potion = new RandomPotion(id: 1, roomId: roomId, fromShop: false);

            var (maxRoll, currentRoll) = potion.Use();

            maxRoll.Should().BeInRange(-maxBase, maxBase * 2);
            currentRoll.Should().BeInRange(-currentBase, currentBase * 2);
            potion.MaxHealthBoost.Should().Be(maxRoll);
            potion.CurrentHealthBoost.Should().Be(currentRoll);
        }
    }
}
