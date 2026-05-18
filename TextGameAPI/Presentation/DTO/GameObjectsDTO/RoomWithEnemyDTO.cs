using TextGame.Presentation.DTO.GameObjectsDTO;

namespace TextGame.Presentation.DTO
{
    public record RoomWithEnemyDTO(int Number, string Name, string Description, EnemyDTO Enemy) 
        : RoomDTOBase(Number, Name, Description);
}