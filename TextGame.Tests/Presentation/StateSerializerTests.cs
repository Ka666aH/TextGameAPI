using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.JSON;

namespace TextGame.Tests.Presentation;

/// <summary>
/// State snapshot serialization. The Deserialize failures are regression guards for
/// notes/AGENTS.md §2.6: a broken payload must surface as a game exception, never as an
/// empty State that silently wipes the player's map.
/// </summary>
public class StateSerializerTests
{
    [Fact]
    public void SerializeThenDeserialize_DefaultState_KeepsCoreValues()
    {
        // Arrange
        var state = new State();

        // Act
        string json = StateSerializer.Serialize(state);
        State restored = StateSerializer.Deserialize(json);

        // Assert
        restored.MaxHealth.Should().Be(state.MaxHealth);
        restored.CurrentHealth.Should().Be(state.CurrentHealth);
        restored.Coins.Should().Be(state.Coins);
        restored.Keys.Should().Be(state.Keys);
        restored.Rooms.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_EmptyString_ThrowsGameException()
    {
        // Arrange
        const string emptyJson = "";

        // Act
        Action act = () => StateSerializer.Deserialize(emptyJson);

        // Assert
        // notes/AGENTS.md §2.6: a broken snapshot must surface as a game exception,
        // not as a raw Newtonsoft reader failure or a silently empty State.
        act.Should().Throw<GameException>();
    }

    [Fact]
    public void Deserialize_NullLiteral_ThrowsGameException()
    {
        // Arrange
        const string jsonNull = "null";

        // Act
        Action act = () => StateSerializer.Deserialize(jsonNull);

        // Assert
        // notes/AGENTS.md §2.6: the old `?? new State()` fallback hid a corrupt cache entry.
        act.Should().Throw<GameException>();
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsGameException()
    {
        // Arrange
        const string malformedJson = "{ \"MaxHealth\": ";

        // Act
        Action act = () => StateSerializer.Deserialize(malformedJson);

        // Assert
        act.Should().Throw<GameException>();
    }
}
