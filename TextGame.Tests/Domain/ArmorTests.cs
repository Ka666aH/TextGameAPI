using TextGame.Domain;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Domain;

/// <summary>
/// Armor behaviour: durability drain, block value, breakage and cost recalculation.
/// </summary>
public class ArmorTests
{
    [Fact]
    public void Block_DecrementsDurabilityAndReturnsBlock()
    {
        // Arrange
        var helm = new ScriptedHelm(durability: 3, damageBlock: 4);

        // Act
        ArmorBlockResult result = helm.Block();

        // Assert
        helm.Durability.Should().Be(2);
        result.Should().Be(new ArmorBlockResult(DamageBlock: 4, IsArmorBrokenDown: false));
    }

    [Fact]
    public void Block_WhenDurabilityReachesZero_ReportsBroken()
    {
        // Arrange
        var helm = new ScriptedHelm(durability: 1, damageBlock: 4);

        // Act
        ArmorBlockResult result = helm.Block();

        // Assert
        helm.Durability.Should().Be(0);
        result.IsArmorBrokenDown.Should().BeTrue();
    }

    [Fact]
    public void Block_AfterBreaking_KeepsReturningBlockAndDrainsBelowZero()
    {
        // Arrange
        var helm = new ScriptedHelm(durability: 1, damageBlock: 4);
        helm.Block();

        // Act
        ArmorBlockResult result = helm.Block();

        // Assert
        helm.Durability.Should().Be(-1, "the domain never removes broken armor by itself");
        result.IsArmorBrokenDown.Should().BeTrue();
        result.DamageBlock.Should().Be(4);
    }

    [Fact]
    public void Block_RecalculatesCostFromRemainingDurability()
    {
        // Arrange
        var helm = new ScriptedHelm(durability: 3, damageBlock: 4);

        // Act
        helm.Block();

        // Assert
        helm.Cost.Should().Be(GameBalance.CalculateArmorCost(durability: 2, damageBlock: 4));
    }

    [Fact]
    public void LeatherHelm_FromRoom_RollsStatsWithinSpread()
    {
        // Arrange
        var (minDurability, maxDurability) = GameBalance.CalculateSpread(GameBalance.LeatherHelmBaseDurability, 0);
        var (minBlock, maxBlock) = GameBalance.CalculateSpread(GameBalance.LeatherHelmBaseDamageBlock, 0);

        // Act
        var helm = new LeatherHelm(id: 1, roomId: 0, fromShop: false);

        // Assert
        helm.Durability!.Value.Should().BeInRange(minDurability, maxDurability);
        helm.DamageBlock.Should().BeInRange(minBlock, maxBlock);
    }

    [Fact]
    public void IronCuirass_FromShop_AppliesShopMultiplier()
    {
        // Arrange
        var (minDurability, maxDurability) = GameBalance.CalculateSpread(GameBalance.IronCuirassBaseDurability, 0);
        var (minBlock, maxBlock) = GameBalance.CalculateSpread(GameBalance.IronCuirassBaseDamageBlock, 0);
        int expectedMinDurability = GameBalance.CalculateShopMultiplier(minDurability);
        int expectedMaxDurability = GameBalance.CalculateShopMultiplier(maxDurability);
        int expectedMinBlock = GameBalance.CalculateShopMultiplier(minBlock);
        int expectedMaxBlock = GameBalance.CalculateShopMultiplier(maxBlock);

        // Act
        var cuirass = new IronCuirass(id: 1, roomId: 0, fromShop: true);

        // Assert
        cuirass.Durability!.Value.Should().BeInRange(expectedMinDurability, expectedMaxDurability);
        cuirass.DamageBlock.Should().BeInRange(expectedMinBlock, expectedMaxBlock);
    }

    [Fact]
    public void WoodenBucket_RollsAtLeastOneDurabilityInStartRoom()
    {
        // Arrange
        var bucket = new WoodenBucket(id: 1, roomId: 0, fromShop: false);

        // Act
        ArmorBlockResult result = bucket.Block();

        // Assert
        result.DamageBlock.Should().BeGreaterThanOrEqualTo(1);
    }
}
