namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public abstract record RoomDTOBase(int Number, string Name, string Description, bool IsSearched, string Kind)
        : GameObjectDTO(Name, Description);
}
