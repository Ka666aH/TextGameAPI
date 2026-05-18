namespace TextGame.Presentation.DTO
{
    public record WeaponDTO(int? Id, string Name, string Description, int? Cost, int? Durability, int? Damage)
        : EquipmentDTO(Id, Name, Description, Cost, Durability);
}