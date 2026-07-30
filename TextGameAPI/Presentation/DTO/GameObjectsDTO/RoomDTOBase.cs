namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public abstract record RoomDTOBase(int Number, string Name, string Description)
        : GameObjectDTO(Name, Description);
}
