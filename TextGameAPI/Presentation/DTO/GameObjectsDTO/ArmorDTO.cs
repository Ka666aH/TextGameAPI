namespace TextGame.Presentation.DTO
{
    public record ArmorDTO(int? Id, string Name, string Description, int? Cost, int? Durability, int? DamageBlock)
        : EquipmentDTO(Id, Name, Description, Cost, Durability);
}