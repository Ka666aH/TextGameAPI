using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;

namespace TextGame.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        void EquipInventoryItem(Equipment equip);
        List<Equipment> GetEquipment();
        void UnequipWeapon();
        void UnequipHelm();
        void UnequipChestplate();
        void SellInventoryItem(Item item);
    }
}