using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.GameExceptions;

namespace TextGame.Tests.Domain;

/// <summary>
/// Chest as an item: locked/closed flags, mimic binding and inventory guards.
/// </summary>
public class ChestTests
{
    [Fact]
    public void NewChest_IsClosedUnlockedCarryableAndUnsellable()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item>());

        // Assert
        chest.IsClosed.Should().BeTrue();
        chest.IsCarryable.Should().BeFalse();
        chest.Cost.Should().BeNull();
        chest.Mimic.Should().BeNull();
    }

    [Fact]
    public void NewChest_WithMimic_ExposesMimicAndItsId()
    {
        // Arrange
        var mimic = new Mimic(id: 7, roomId: 0);

        // Act
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item>(), mimic: mimic);

        // Assert
        chest.Mimic.Should().BeSameAs(mimic);
        chest.MimicId.Should().Be(7);
    }

    [Fact]
    public void KillMimic_ClearsMimicUnlocksAndOpens()
    {
        // Arrange
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item>(), mimic: new Mimic(id: 7, roomId: 0));
        string nameBefore = chest.Name;

        // Act
        chest.KillMimic();

        // Assert
        chest.Mimic.Should().BeNull();
        chest.IsLocked.Should().BeFalse();
        chest.IsClosed.Should().BeFalse();
        chest.Name.Should().NotBe(nameBefore, "a dead mimic chest is renamed to the dead-mimic variant");
    }

    [Fact]
    public void Open_ClearsClosedFlag()
    {
        // Arrange
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item>());

        // Act
        chest.Open();

        // Assert
        chest.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void Unlock_ClearsLockedFlag()
    {
        // Arrange
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item>());

        // Act
        chest.Unlock();

        // Assert
        chest.IsLocked.Should().BeFalse();
    }

    [Fact]
    public void RemoveItem_NotInChest_ThrowsItemNotFoundException()
    {
        // Arrange
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item>());
        var key = new Key(id: 1, roomId: 0);

        // Act
        Action act = () => chest.RemoveItem(key);

        // Assert
        act.Should().Throw<ItemNotFoundException>();
    }

    [Fact]
    public void RemoveAllItems_EmptiesTheChest()
    {
        // Arrange
        var items = new List<Item> { new Key(id: 1, roomId: 0), new Map(id: 2, roomId: 0) };
        var chest = new Chest(id: 1, roomId: 0, items: items);

        // Act
        chest.RemoveAllItems();

        // Assert
        chest.Items.Should().BeEmpty();
    }

    [Fact]
    public void Search_ReturnsChestContents()
    {
        // Arrange
        var key = new Key(id: 1, roomId: 0);
        var chest = new Chest(id: 1, roomId: 0, items: new List<Item> { key });

        // Act
        IReadOnlyList<Item> found = chest.Search();

        // Assert
        found.Should().ContainSingle().Which.Should().BeSameAs(key);
    }
}
