namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record ChestDTO(int? Id, string Name, string Description, bool IsClosed)
        : GameObjectDTO(Name, Description);
}