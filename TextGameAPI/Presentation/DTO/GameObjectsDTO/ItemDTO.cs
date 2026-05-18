namespace TextGame.Presentation.DTO
{
    public record ItemDTO(int? Id, string Name, string Description, int? Cost)
        : GameObjectDTO(Name, Description);
}