using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;

namespace TextGame.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Item GetInventoryItem(int itemId);
        List<Equipment> EquipInventoryItem(int itemId);
        List<Equipment> GetEquipment();
        List<Equipment> UnequipWeapon();
        List<Equipment> UnequipHelm();
        List<Equipment> UnequipChestplate();
        void SellInventoryItem(int itemId);
        //List<Item> GetInventoryItems(List<int> itemIds);
    }
}