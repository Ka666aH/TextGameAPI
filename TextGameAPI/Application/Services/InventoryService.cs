using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;

namespace TextGame.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IStateService _stateService;
        public InventoryService(IStateService stateService)
        {
            _stateService = stateService;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = [_stateService.Weapon];
            if (_stateService.Helm != null) equipmentList.Add(_stateService.Helm);
            if (_stateService.Chestplate != null) equipmentList.Add(_stateService.Chestplate);
            return equipmentList;
        }
        public void EquipInventoryItem(Equipment equip)
        {
            switch (equip)
            {
                case Weapon weapon:
                    if (_stateService.Weapon != Fists.DefaultFists) _stateService.AddItemToInventory(_stateService.Weapon);
                    _stateService.EquipWeapon(weapon);
                    _stateService.RemoveItemFromInventory(weapon);
                    break;
                case Armor armor:
                    switch (armor)
                    {
                        case Helm helm:
                            if (_stateService.Helm != null) _stateService.AddItemToInventory(_stateService.Helm);
                            _stateService.EquipHelm(helm);
                            _stateService.RemoveItemFromInventory(helm);
                            break;
                        case Chestplate chestplate:
                            if (_stateService.Chestplate != null) _stateService.AddItemToInventory(_stateService.Chestplate);
                            _stateService.EquipChestplate(chestplate);
                            _stateService.RemoveItemFromInventory(chestplate);
                            break;
                    }
                    break;
            }
        }
        public void UnequipWeapon()
        {
            if (_stateService.Weapon == Fists.DefaultFists) throw new EmptyException();

            _stateService.AddItemToInventory(_stateService.Weapon);
            _stateService.RemoveWeapon();
        }
        public void UnequipHelm()
        {
            if (_stateService.Helm == null) throw new EmptyException();

            _stateService.AddItemToInventory(_stateService.Helm);
            _stateService.RemoveHelm();
        }
        public void UnequipChestplate()
        {
            if (_stateService.Chestplate == null) throw new EmptyException();

            _stateService.AddItemToInventory(_stateService.Chestplate);
            _stateService.RemoveChestplate();
        }
        public void SellInventoryItem(Item item)
        {
            if (item.Cost == null) throw new UnsellableItemException();

            _stateService.RemoveItemFromInventory(item);
            _stateService.AddCoins((int)item.Cost);

            item.AddStoreMargin();
            _stateService.AddItemToCurrentRoom(item);
        }
    }
}