using TextGame.Infrastructure.Token.JWT;

namespace TextGame.Tests.Infrastructure;

/// <summary>
/// notes/AGENTS.md §1b: a JWT secret that HMAC-SHA256 cannot use must fail at startup, not on the
/// first login. Only the rejection paths are asserted here: the success path would overwrite the
/// process-wide key the integration host signs tokens with, and that would make parallel suites flaky.
/// </summary>
public class JwtKeyProviderTests
{
    [Fact]
    public void Initialize_WithSecretShorterThanMinimum_Throws()
    {
        // Arrange
        string shortSecret = new('x', JwtKeyProvider.MinimumSecretLength - 1);

        // Act
        Action act = () => JwtKeyProvider.Initialize(shortSecret);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*too short*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Initialize_WithEmptyOrBlankSecret_Throws(string secret)
    {
        // Arrange
        // (secret comes from the test data)

        // Act
        Action act = () => JwtKeyProvider.Initialize(secret);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*not configured*");
    }
}
