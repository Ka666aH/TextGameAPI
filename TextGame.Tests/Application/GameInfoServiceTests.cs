using TextGame.Application.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Application;

/// <summary>
/// Payload of GameInfoDTO is assembled by the real service here: the orchestrator tests use
/// FakeGameInfoService and only assert the exception type, so the wire values of
/// <c>isGameStarted</c> and <c>result</c> are pinned in this file.
/// </summary>
public class GameInfoServiceTests
{
    [Fact]
    public void GetGameInfo_WhenGameIsInProgress_ReturnsStartedWithNullResult()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var service = new GameInfoService(new FakeStateService(state));

        // Act
        var info = service.GetGameInfo();

        // Assert
        info.IsGameStarted.Should().BeTrue();
        info.Result.Should().BeNull();
    }

    [Theory]
    [InlineData(GameResult.Win, "win")]
    [InlineData(GameResult.Lose, "lose")]
    public void GetGameInfo_WhenGameIsOver_ReturnsWireResultAsString(GameResult result, string expected)
    {
        // Arrange
        var state = new State { IsGameStarted = false, Result = result };
        state.Rooms.Add(new StartRoom());
        var service = new GameInfoService(new FakeStateService(state));

        // Act
        var info = service.GetGameInfo();

        // Assert
        info.IsGameStarted.Should().BeFalse();
        info.Result.Should().Be(expected);
    }

    [Fact]
    public void GetGameInfo_WhenStatePredatesResult_ReturnsNullResult()
    {
        // Arrange
        // Старый JSON не содержит Result: enum остаётся None, а игра уже окончена.
        var state = new State { IsGameStarted = false };
        state.Rooms.Add(new StartRoom());
        var service = new GameInfoService(new FakeStateService(state));

        // Act
        var info = service.GetGameInfo();

        // Assert
        info.IsGameStarted.Should().BeFalse();
        info.Result.Should().BeNull();
    }
}
