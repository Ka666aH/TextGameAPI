namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record ChestDTO(int? Id, string Name, string Description, bool IsClosed, int? Cost, bool IsLocked)
        : GameObjectDTO(Name, Description);
}
