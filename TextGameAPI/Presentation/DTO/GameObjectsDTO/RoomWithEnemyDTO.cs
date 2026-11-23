namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record RoomWithEnemyDTO(int Number, string Name, string Description, bool IsSearched, string Kind, EnemyDTO? Enemy) 
        : RoomDTOBase(Number, Name, Description, IsSearched, Kind);
}
