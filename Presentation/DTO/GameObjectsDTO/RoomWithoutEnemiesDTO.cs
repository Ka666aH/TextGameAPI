using TextGame.Presentation.DTO.GameObjectsDTO;

namespace TextGame.Presentation.DTO
{
    public record RoomWithoutEnemiesDTO(int Number, string Name, string Description)
        : RoomDTOBase(Number, Name, Description);
}