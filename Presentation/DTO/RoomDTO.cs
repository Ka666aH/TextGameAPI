using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Presentation.DTO
{
    public record RoomDTO(int Number, string Name, string Description, IEnumerable<Enemy> Enemies);
}