using TextGame.Presentation.DTO.GameObjectsDTO;

namespace TextGame.Application.DTO
{
    public record GameInfoDTO(RoomDTOBase Room, object Weapon, ArmorDTO? Helm, ArmorDTO? Chestplate, int MaxHealth, int CurrentHealth, int Coins, int Keys, IEnumerable<object> Inventory);
}