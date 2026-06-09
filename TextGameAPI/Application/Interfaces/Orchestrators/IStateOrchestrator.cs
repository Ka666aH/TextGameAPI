using TextGame.Application.DTO;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Orchestrators
{
    public interface IStateOrchestrator
    {
        GameInfoDTO GetGameInfo();
        IEnumerable<Item> GetInventory();
        int GetCoins();
        int GetKeys();
        List<MapRoomDTO> GetMap();
        void UseInventoryItem(int itemId);
        Item GetInventoryItem(int itemId);
        List<Equipment> GetEquipment();
        void EquipInventoryItem(int itemId);
        void UnequipWeapon();
        void UnequipHelm();
        void UnequipChestplate();
        void SellInventoryItem(int itemId);
        Room GetCurrentRoom();
        Room GoNextRoom();
        Room GoToRoom(int roomId);
        List<Item> Search();
        void TakeItem(int itemId);
        void TakeAllItems();
        void BuyItem(int itemId);
        Enemy GetEnemy();
        BattleLog DealDamage();
        BattleLog GetDamage();
        BattleLog HitChest(int chestId);
        Chest UnlockChest(int chestId);
        void OpenChest(int chestId);
        List<Item> SearchChest(int chestId);
        void TakeItemFromChest(int chestId, int itemId);
        void TakeAllItemsFromChest(int chestId);
    }
}
