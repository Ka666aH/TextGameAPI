namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record ItemDTO(int? Id, string Name, string Description, int? Cost)
        : GameObjectDTO(Name, Description);
}