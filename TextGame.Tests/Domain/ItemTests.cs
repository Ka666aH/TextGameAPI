using TextGame.Domain;
using TextGame.Domain.Entities.GameObjects.Items.Other;

namespace TextGame.Tests.Domain;

/// <summary>
/// Item pricing: the shop margin round trip and the per-item cost rules.
/// </summary>
public class ItemTests
{
    [Fact]
    public void AddStoreMargin_DoublesCost()
    {
        // Arrange
        var key = new Key(id: 1, roomId: 0);
        int originalCost = key.Cost!.Value;

        // Act
        key.AddStoreMargin();

        // Assert
        key.Cost.Should().Be(originalCost * (int)GameBalance.StoreMargin);
    }

    [Fact]
    public void RemoveStoreMargin_RestoresOriginalCost()
    {
        // Arrange
        var map = new Map(id: 1, roomId: 0);
        int originalCost = map.Cost!.Value;

        // Act
        map.AddStoreMargin();
        map.RemoveStoreMargin();

        // Assert
        map.Cost.Should().Be(originalCost);
    }

    [Fact]
    public void Key_CostGrowsWithRoomId()
    {
        // Arrange
        var (min, max) = GameBalance.CalculateSpread(GameBalance.KeyBaseCost, 0);

        // Act
        var startRoomKey = new Key(id: 1, roomId: 0);
        var roomFiftyKey = new Key(id: 2, roomId: 50);

        // Assert
        startRoomKey.Cost.Should().Be((int)(GameBalance.KeyBaseCost * GameBalance.CalculateGain(0)));
        startRoomKey.Cost!.Value.Should().BeInRange(min, max);
        roomFiftyKey.Cost.Should().BeGreaterThan(startRoomKey.Cost!.Value);
    }

    [Fact]
    public void Map_KeepsFixedCostAnywhereOnTheMap()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var nearMap = new Map(id: 1, roomId: 0);
        var farMap = new Map(id: 2, roomId: 500);

        // Assert
        nearMap.Cost.Should().Be(GameBalance.MapBaseCost);
        farMap.Cost.Should().Be(GameBalance.MapBaseCost);
    }

    [Fact]
    public void BagOfCoins_CostStaysInsideStartRoomSpread()
    {
        // Arrange
        var (min, max) = GameBalance.CalculateSpread(GameBalance.BagOfCoinsBaseCost, 0);

        // Act
        var bag = new BagOfCoins(id: 1, roomId: 0);

        // Assert
        bag.Cost!.Value.Should().BeInRange(min, max);
    }

    [Fact]
    public void Key_IsCarryableByDefault()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var key = new Key(id: 1, roomId: 0);

        // Assert
        key.IsCarryable.Should().BeTrue();
    }
}
