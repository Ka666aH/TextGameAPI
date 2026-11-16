using TextGame.Application.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Application;

/// <summary>
/// Room lookup guards. The last test is a regression guard for notes/AGENTS.md §1.1: the upper
/// range check must reject the id that equals the room count.
/// </summary>
public class GetRoomServiceTests
{
    [Fact]
    public void GetRoom_WhenRoomIsDiscovered_ReturnsIt()
    {
        // Arrange
        var stateService = CreateStateWithThreeRooms();
        var service = new GetRoomService(stateService);

        // Act
        Room room = service.GetRoom(0);

        // Assert
        room.Should().BeOfType<StartRoom>();
    }

    [Fact]
    public void GetRoom_WhenRoomIsUndiscovered_ThrowsUndiscoveredRoomException()
    {
        // Arrange
        var stateService = CreateStateWithThreeRooms();
        var service = new GetRoomService(stateService);

        // Act
        Action act = () => service.GetRoom(1);

        // Assert
        act.Should().Throw<UndiscoveredRoomException>();
    }

    [Fact]
    public void GetRoom_WhenIdIsNegative_ThrowsRoomNotFoundException()
    {
        // Arrange
        var stateService = CreateStateWithThreeRooms();
        var service = new GetRoomService(stateService);

        // Act
        Action act = () => service.GetRoom(-1);

        // Assert
        act.Should().Throw<RoomNotFoundException>();
    }

    [Fact]
    public void GetRoom_WhenIdIsAboveRoomCount_ThrowsRoomNotFoundException()
    {
        // Arrange
        var stateService = CreateStateWithThreeRooms();
        var service = new GetRoomService(stateService);

        // Act
        Action act = () => service.GetRoom(10);

        // Assert
        act.Should().Throw<RoomNotFoundException>();
    }

    [Fact]
    public void GetRoom_WhenIdEqualsRoomCount_ThrowsRoomNotFoundException()
    {
        // Arrange
        var stateService = CreateStateWithThreeRooms();
        var service = new GetRoomService(stateService);

        // Act
        Action act = () => service.GetRoom(3);

        // Assert
        // notes/AGENTS.md §1.1: the guard is `roomId > Rooms.Count`, so index == Count
        // slips through and crashes with ArgumentOutOfRangeException instead of 404.
        act.Should().Throw<RoomNotFoundException>();
    }

    private static FakeStateService CreateStateWithThreeRooms()
    {
        var state = new State();
        state.Rooms.Add(new StartRoom());
        state.Rooms.Add(new EmptyRoom(id: 1));
        state.Rooms.Add(new EmptyRoom(id: 2));

        return new FakeStateService(state);
    }
}
