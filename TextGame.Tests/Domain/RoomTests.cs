using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Domain;

/// <summary>
/// Room as a container: discovery, search flag, contents and removal guards.
/// </summary>
public class RoomTests
{
    [Fact]
    public void NewRoom_IsUndiscoveredAndUnsearched()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var room = new EmptyRoom(id: 1);

        // Assert
        room.Id.Should().Be(1);
        room.IsDiscovered.Should().BeFalse();
        room.IsSearched.Should().BeFalse();
    }

    [Fact]
    public void StartRoom_StartsDiscovered()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var room = new StartRoom();

        // Assert
        room.Id.Should().Be(0);
        room.IsDiscovered.Should().BeTrue();
    }

    [Fact]
    public void Discover_SetsDiscoveredFlag()
    {
        // Arrange
        var room = new EmptyRoom(id: 1);

        // Act
        room.Discover();

        // Assert
        room.IsDiscovered.Should().BeTrue();
    }

    [Fact]
    public void Search_MarksRoomSearched_AndReturnsItsItems()
    {
        // Arrange
        var room = new SmallRoom(id: 2);
        var key = new Key(id: 1, roomId: 2);
        room.AddItem(key);

        // Act
        IReadOnlyList<Item> found = room.Search();

        // Assert
        room.IsSearched.Should().BeTrue();
        found.Should().ContainSingle().Which.Should().BeSameAs(key);
    }

    [Fact]
    public void AddItem_BecomesVisibleThroughItems()
    {
        // Arrange
        var room = new EmptyRoom(id: 1);
        var key = new Key(id: 1, roomId: 1);

        // Act
        room.AddItem(key);

        // Assert
        room.Items.Should().ContainSingle().Which.Should().BeSameAs(key);
    }

    [Fact]
    public void RemoveItem_NotInRoom_ThrowsItemNotFoundException()
    {
        // Arrange
        var room = new EmptyRoom(id: 1);
        var key = new Key(id: 1, roomId: 1);

        // Act
        Action act = () => room.RemoveItem(key);

        // Assert
        act.Should().Throw<ItemNotFoundException>();
    }

    [Fact]
    public void RemoveItem_InRoom_RemovesIt()
    {
        // Arrange
        var room = new EmptyRoom(id: 1);
        var key = new Key(id: 1, roomId: 1);
        room.AddItem(key);

        // Act
        room.RemoveItem(key);

        // Assert
        room.Items.Should().BeEmpty();
    }

    [Fact]
    public void Enemy_ReturnsFirstEnemyOfTheRoom()
    {
        // Arrange
        var room = new BigRoom(id: 3);
        var enemy = new TestEnemy(health: 10, damage: 1, damageBlock: 0, roomId: 3);

        // Act
        room.AddEnemy(enemy);

        // Assert
        room.Enemy.Should().BeSameAs(enemy);
        room.Enemies.Should().ContainSingle();
    }

    [Fact]
    public void RemoveEnemy_NotInRoom_ThrowsEnemyNotFoundException()
    {
        // Arrange
        var room = new BigRoom(id: 3);
        Enemy enemy = new TestEnemy();

        // Act
        Action act = () => room.RemoveEnemy(enemy);

        // Assert
        act.Should().Throw<EnemyNotFoundException>();
    }

    [Fact]
    public void RemoveEnemy_InRoom_ClearsCurrentEnemy()
    {
        // Arrange
        var room = new BigRoom(id: 3);
        Enemy enemy = new TestEnemy(roomId: 3);
        room.AddEnemy(enemy);

        // Act
        room.RemoveEnemy(enemy);

        // Assert
        room.Enemy.Should().BeNull();
    }
}
