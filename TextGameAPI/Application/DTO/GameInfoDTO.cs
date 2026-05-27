using TextGame.Presentation.DTO.GameObjectsDTO;

namespace TextGame.Application.DTO
{
    public record GameInfoDTO(RoomDTOBase Room, IGameObjectDTO Weapon, ArmorDTO? Helm, ArmorDTO? Chestplate, int MaxHealth, int CurrentHealth, int Coins, int Keys, IEnumerable<IGameObjectDTO> Inventory);
}