using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Presentation.DTO.GameObjectsDTO;
using TextGame.Presentation.Mappers;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Presentation;

/// <summary>
/// Room DTO contract for 1.2.0: <c>kind</c> comes from the CLR room type and <c>isSearched</c>
/// from the domain flag, so the client stops matching on Russian room names and stops caching
/// the search flag itself (notes/backend-requests.md п. 6, 7).
/// </summary>
public class GameObjectMapperTests
{
    [Fact]
    public void ToDTO_ReturnsKindForEveryRoomType()
    {
        // Arrange
        (Room Room, string Kind)[] cases =
        [
            (new StartRoom(), "start"),
            (new EmptyRoom(id: 1), "empty"),
            (new SmallRoom(id: 2), "small"),
            (new BigRoom(id: 3), "big"),
            (new Shop(id: 4), "shop"),
            (new EndRoom(id: 5), "exit"),
        ];

        // Act + Assert
        foreach (var (room, kind) in cases)
        {
            var dto = (RoomDTOBase)room.ToDTO();
            dto.Kind.Should().Be(kind, "вид комнаты выводится из типа {0}", room.GetType().Name);
        }
    }

    [Fact]
    public void ToDTO_WhenRoomIsSearched_ReturnsIsSearchedTrue()
    {
        // Arrange
        var room = new SmallRoom(id: 1);

        // Act
        var before = (RoomDTOBase)room.ToDTO();
        room.Search();
        var after = (RoomDTOBase)room.ToDTO();

        // Assert
        before.IsSearched.Should().BeFalse();
        after.IsSearched.Should().BeTrue();
    }

    [Fact]
    public void ToDTO_WhenRoomHasEnemy_ReturnsKindWithEnemy()
    {
        // Arrange
        var room = new BigRoom(id: 1);
        room.AddEnemy(new TestEnemy(roomId: 1));

        // Act
        var dto = room.ToDTO();

        // Assert
        var withEnemy = dto.Should().BeOfType<RoomWithEnemyDTO>().Which;
        withEnemy.Kind.Should().Be("big");
        withEnemy.Enemy.Should().NotBeNull();
    }

    [Fact]
    public void ToDTO_WhenRoomHasNoEnemy_ReturnsKindWithoutEnemy()
    {
        // Arrange
        var room = new EmptyRoom(id: 1);

        // Act
        var dto = room.ToDTO();

        // Assert
        var withoutEnemy = dto.Should().BeOfType<RoomWithoutEnemyDTO>().Which;
        withoutEnemy.Kind.Should().Be("empty");
    }
}
