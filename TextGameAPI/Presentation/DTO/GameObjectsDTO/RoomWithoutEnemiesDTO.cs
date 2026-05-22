namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record RoomWithoutEnemiesDTO(int Number, string Name, string Description)
        : RoomDTOBase(Number, Name, Description);
}