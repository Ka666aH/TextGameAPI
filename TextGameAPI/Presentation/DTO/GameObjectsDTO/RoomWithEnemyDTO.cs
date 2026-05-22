namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record RoomWithEnemyDTO(int Number, string Name, string Description, EnemyDTO? Enemy) 
        : RoomDTOBase(Number, Name, Description);
}