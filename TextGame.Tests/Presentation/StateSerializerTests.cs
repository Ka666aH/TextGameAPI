using TextGame.Domain.Entities;
using TextGame.Domain.GameExceptions;
using TextGame.Infrastructure.JSON;

namespace TextGame.Tests.Presentation;

/// <summary>
/// State snapshot serialization. The three red tests cover notes/AGENTS.md §2.6:
/// broken payloads are silently turned into an empty State instead of failing loudly.
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
    [Trait("Category", "KnownBug")]
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
    [Trait("Category", "KnownBug")]
    public void Deserialize_NullLiteral_ThrowsGameException()
    {
        // Arrange
        const string jsonNull = "null";

        // Act
        Action act = () => StateSerializer.Deserialize(jsonNull);

        // Assert
        // notes/AGENTS.md §2.6: the `?? new State()` fallback hides a corrupt cache entry.
        act.Should().Throw<GameException>();
    }

    [Fact]
    [Trait("Category", "KnownBug")]
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
