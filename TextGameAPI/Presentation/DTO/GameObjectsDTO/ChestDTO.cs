namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record ChestDTO(int? Id, string Name, string Description, bool IsClosed)
        : ItemDTO(Id, Name, Description, null);
}