using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Presentation.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameControllerService : IGameInfoService
    {
        //void Start();
        IEnumerable<Item> GetInventory();
        int GetCoins();
        int GetKeys();
        List<MapRoomDTO> GetMap();
        void UseInventoryItem(int itemId);

        Room GetCurrentRoom();
        Item GetInventoryItem(int itemId);

        List<Equipment> GetEquipment();
        void EquipInventoryItem(int itemId);
        void UnequipWeapon();
        void UnequipHelm();
        void UnequipChestplate();
        void SellInventoryItem(int itemId);
    }
}