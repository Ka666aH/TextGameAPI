using TextGame.Presentation.DTO.GameObjectsDTO;

namespace TextGame.Presentation.DTO
{
    public record GameInfoDTO(RoomDTOBase Room, WeaponDTO Weapon, ArmorDTO? Helm, ArmorDTO? Chestplate, int MaxHealth, int CurrentHealth, int Coins, int Keys, IEnumerable<ItemDTO> Inventory);
}