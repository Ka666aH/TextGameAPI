namespace TextGame.Presentation.DTO
{
    public record GameInfoDTO(object room, WeaponDTO Weapon, ArmorDTO? Helm, ArmorDTO? Chestplate, int MaxHealth, int CurrentHealth, int coins, int keys, IEnumerable<object> Inventory);
}