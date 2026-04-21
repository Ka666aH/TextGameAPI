using TextGame.Presentation.DTO.GameObjectsDTO;

namespace TextGame.Presentation.DTO
{
    public record RoomWithEnemiesDTO(int Number, string Name, string Description, IEnumerable<EnemyDTO> Enemies) 
        : RoomDTOBase(Number, Name, Description);
}