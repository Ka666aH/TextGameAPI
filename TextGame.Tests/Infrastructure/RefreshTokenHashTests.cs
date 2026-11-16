using TextGame.Infrastructure.Token;

namespace TextGame.Tests.Infrastructure;

/// <summary>
/// At-rest hashing of refresh tokens: the database never sees the raw value, and lookups stay
/// deterministic so the unique index on the stored value keeps working (notes/AGENTS.md §1b, п.8).
/// </summary>
public class RefreshTokenHashTests
{
    [Fact]
    public void Of_IsDeterministic_ForTheSameInput()
    {
        // Arrange
        const string rawToken = "same-raw-token";

        // Act
        string first = RefreshTokenHash.Of(rawToken);
        string second = RefreshTokenHash.Of(rawToken);

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void Of_Produces64HexCharacters()
    {
        // Arrange
        const string rawToken = "any-token";

        // Act
        string hash = RefreshTokenHash.Of(rawToken);

        // Assert
        // SHA-256 as uppercase hex: fits the Token column and stays index-friendly.
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    [Fact]
    public void Of_DifferentInputs_ProduceDifferentHashes()
    {
        // Arrange
        // (two raw tokens below)

        // Act
        string first = RefreshTokenHash.Of("token-one");
        string second = RefreshTokenHash.Of("token-two");

        // Assert
        first.Should().NotBe(second);
    }
}
