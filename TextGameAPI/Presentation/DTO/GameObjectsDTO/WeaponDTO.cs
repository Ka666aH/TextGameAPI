namespace TextGame.Presentation.DTO.GameObjectsDTO
{
    public record WeaponDTO(int? Id, string Name, string Description, int? Cost, int? Durability, int? Damage)
        : EquipmentDTO(Id, Name, Description, Cost, Durability);
}