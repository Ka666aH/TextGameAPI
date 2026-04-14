namespace TextGame.Presentation.DTO
{
    public record WeaponDTO(int? Id, string Name, string Description, int? cost, int? Durability, int? Damage);
}