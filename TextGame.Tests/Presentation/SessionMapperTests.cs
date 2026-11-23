using TextGame.Domain.Entities;
using TextGame.Presentation.Mappers;

namespace TextGame.Tests.Presentation;

/// <summary>
/// notes/backend-requests.md п. 15: <c>seed</c> уезжает строкой. <c>long</c> в JSON-числе
/// теряет точность в JS за 2^53, поэтому проверяем сид, который в число не влезает.
/// </summary>
public class SessionMapperTests
{
    [Fact]
    public void ToDTO_WhenSeedExceedsJsSafeInteger_ReturnsExactString()
    {
        // Arrange
        const long seed = 9007199254740993L;
        var session = new Session(Guid.NewGuid(), seed: seed);

        // Act
        var dto = session.ToDTO();

        // Assert
        dto.Seed.Should().Be("9007199254740993");
    }

    [Fact]
    public void ToDTO_WhenSeedIsNegative_ReturnsInvariantString()
    {
        // Arrange
        var session = new Session(Guid.NewGuid(), seed: -42L);

        // Act
        var dto = session.ToDTO();

        // Assert
        dto.Seed.Should().Be("-42");
    }
}
