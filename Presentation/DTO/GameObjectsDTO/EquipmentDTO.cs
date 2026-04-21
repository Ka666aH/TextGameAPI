namespace TextGame.Presentation.DTO
{
    public record EquipmentDTO(int? Id, string Name, string Description, int? Cost, int? Durability)
        : ItemDTO(Id, Name, Description, Cost);
}