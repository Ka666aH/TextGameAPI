namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record RoomWithoutEnemyDTO(int Number, string Name, string Description)
        : RoomDTOBase(Number, Name, Description);
}