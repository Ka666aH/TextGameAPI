namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record WandDTO(int? Id, string Name, string Description, int? Cost, int? Damage)
        : ItemDTO(Id, Name, Description, Cost);
}