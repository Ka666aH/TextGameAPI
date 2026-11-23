namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record RoomWithoutEnemyDTO(int Number, string Name, string Description, bool IsSearched, string Kind)
        : RoomDTOBase(Number, Name, Description, IsSearched, Kind);
}
