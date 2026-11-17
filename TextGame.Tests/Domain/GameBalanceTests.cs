using TextGame.Domain;

namespace TextGame.Tests.Domain;

/// <summary>
/// Formulas of the game balance. Pure static class: no setup, no doubles.
/// </summary>
public class GameBalanceTests
{
    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(10, 1.1)]
    [InlineData(50, 1.5)]
    [InlineData(100, 2.0)]
    public void CalculateGain_AddsOnePercentPerRoom(int roomId, double expected)
    {
        // Arrange
        // (nothing to arrange: pure function of roomId)

        // Act
        double gain = GameBalance.CalculateGain(roomId);

        // Assert
        gain.Should().BeApproximately(expected, 1e-12);
    }

    [Theory]
    [InlineData(100, 0, 80, 120)]
    [InlineData(20, 0, 16, 24)]
    [InlineData(12, 0, 9, 14)]
    [InlineData(4, 0, 3, 4)]
    [InlineData(100, 10, 88, 132)]
    public void CalculateSpread_UsesFloorAndCeilingAroundGainedValue(int baseValue, int roomId, int expectedMin, int expectedMax)
    {
        // Arrange
        // (nothing to arrange: pure function)

        // Act
        var (min, max) = GameBalance.CalculateSpread(baseValue, roomId);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    [Fact]
    public void CalculateSpread_OfValueOne_TruncatesMinimumToZero()
    {
        // Arrange
        const int baseValue = 1;
        const int roomId = 0;

        // Act
        var (min, max) = GameBalance.CalculateSpread(baseValue, roomId);

        // Assert
        min.Should().Be(0, "0.8 truncates down to 0, so a value of 1 can roll to zero");
        max.Should().Be(1);
    }

    [Fact]
    public void CalculateSpread_GrowsMonotonicallyWithRoomId()
    {
        // Arrange
        const int baseValue = 100;
        var previousMax = 0;

        // Act + Assert
        foreach (int roomId in Enumerable.Range(0, 200))
        {
            var (min, max) = GameBalance.CalculateSpread(baseValue, roomId);

            min.Should().BeLessThanOrEqualTo(max);
            max.Should().BeGreaterThanOrEqualTo(previousMax);
            previousMax = max;
        }
    }

    [Theory]
    [InlineData(10, 12)]
    [InlineData(50, 60)]
    [InlineData(100, 120)]
    public void CalculateShopMultiplier_IncreasesValueByTwentyPercent(int baseValue, int expected)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int multiplied = GameBalance.CalculateShopMultiplier(baseValue);

        // Assert
        multiplied.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 20, 10)]
    [InlineData(10, 30, 40)]
    [InlineData(0, 0, 0)]
    public void CalculateHealCost_WeightsMaxBoostFiveTimesHigherThanCurrentBoost(int? maxHealthBoost, int? currentHealthBoost, int expected)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int cost = GameBalance.CalculateHealCost(maxHealthBoost, currentHealthBoost);

        // Assert
        cost.Should().Be(expected);
    }

    [Fact]
    public void CalculateHealCost_WithoutBoosts_ReturnsZero()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int cost = GameBalance.CalculateHealCost(null, null);

        // Assert
        cost.Should().Be(0);
    }

    [Fact]
    public void CalculateSwordCost_CombinesDurabilityAndDamageSquares()
    {
        // Arrange
        const int durability = 8;
        const int damage = 15;

        // Act
        int cost = GameBalance.CalculateSwordCost(durability, damage);

        // Assert
        // 1 + (8 * 15 / 15) + (15 * 15 / 150) = 1 + 8 + 1
        cost.Should().Be(10);
    }

    [Theory]
    [InlineData(12, 4, 13)]
    [InlineData(9, 3, 7)]
    public void CalculateArmorCost_IsOnePlusQuarterOfDurabilityTimesBlock(int durability, int damageBlock, int expected)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int cost = GameBalance.CalculateArmorCost(durability, damageBlock);

        // Assert
        cost.Should().Be(expected);
    }

    [Fact]
    public void CalculateWandCost_IsTwiceTheDamage()
    {
        // Arrange
        const int damage = 21;

        // Act
        int cost = GameBalance.CalculateWandCost(damage);

        // Assert
        cost.Should().Be(42);
    }

    [Fact]
    public void CalculateRandomPotionBaseCost_IsFortyFour()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int baseCost = GameBalance.CalculateRandomPotionBaseCost();

        // Assert
        // (1 + 12.5 * 5 + 25 * 1) / 2 = 88 / 2
        baseCost.Should().Be(44);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(50, 40)]
    [InlineData(99, 20)]
    [InlineData(100, 20)]
    public void CalculateNoneWeight_DropsToTwentyFromRoomHundred(int roomId, int expected)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int weight = GameBalance.CalculateNoneWeight(roomId);

        // Assert
        weight.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(25, 12)]
    [InlineData(49, 0)]
    [InlineData(50, 0)]
    public void CalculateSkeletorWeight_ReachesZeroBeforeRoomFifty(int roomId, int expected)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int weight = GameBalance.CalculateSkeletorWeight(roomId);

        // Assert
        weight.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 15)]
    [InlineData(64, 0)]
    [InlineData(65, 0)]
    public void CalculateSkeletorArcherWeight_IsZeroFromRoomSixtyFive(int roomId, int expected)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int weight = GameBalance.CalculateSkeletorArcherWeight(roomId);

        // Assert
        weight.Should().Be(expected);
    }

    [Fact]
    public void CalculateGhostWeight_IsZeroInStartRoom()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int weight = GameBalance.CalculateGhostWeight(0);

        // Assert
        weight.Should().Be(0);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    public void CalculateLichWeight_StaysZeroUntilRoomFifteen(int roomId)
    {
        // Arrange
        // (nothing to arrange)

        // Act
        int weight = GameBalance.CalculateLichWeight(roomId);

        // Assert
        weight.Should().Be(0, "0.21053 per room truncates to 0 for the first four rooms after ten");
    }

    [Fact]
    public void RoomItemWeights_AreNeverNegative()
    {
        // Arrange
        var weightFunctions = new (string Name, Func<int, int> Weight)[]
        {
            ("None", GameBalance.CalculateNoneWeight),
            ("Skeletor", GameBalance.CalculateSkeletorWeight),
            ("SkeletorArcher", GameBalance.CalculateSkeletorArcherWeight),
            ("Deadman", GameBalance.CalculateDeadmanWeight),
            ("Ghost", GameBalance.CalculateGhostWeight),
            ("Lich", GameBalance.CalculateLichWeight),
            ("RustSword", GameBalance.CalculateRustSwordRoomWeight),
            ("WoodenBucket", GameBalance.CalculateWoodenBucketRoomWeight),
            ("Bandage", GameBalance.CalculateBandageRoomWeight),
            ("LeatherHelmChest", GameBalance.CalculateLeatherHelmChestWeight),
            ("RustSwordChest", GameBalance.CalculateRustSwordChestWeight),
            ("RustSwordShop", GameBalance.CalculateRustSwordShopWeight),
            ("BandageShop", GameBalance.CalculateBandageShopWeight),
        };

        // Act + Assert
        foreach (var (name, weight) in weightFunctions)
        {
            foreach (int roomId in Enumerable.Range(0, 301))
            {
                weight(roomId).Should().BeGreaterThanOrEqualTo(0, $"{name} weight at room {roomId} must not be negative");
            }
        }
    }
}
