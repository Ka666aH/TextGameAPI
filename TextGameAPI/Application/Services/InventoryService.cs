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
        private readonly IGameSessionService _gameSessionService;
        public InventoryService(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = [_gameSessionService.Weapon];
            if (_gameSessionService.Helm != null) equipmentList.Add(_gameSessionService.Helm);
            if (_gameSessionService.Chestplate != null) equipmentList.Add(_gameSessionService.Chestplate);
            return equipmentList;
        }
        public void EquipInventoryItem(Equipment equip)
        {
            switch (equip)
            {
                case Weapon weapon:
                    if (_gameSessionService.Weapon != Fists.DefaultFists) _gameSessionService.AddItemToInventory(_gameSessionService.Weapon);
                    _gameSessionService.EquipWeapon(weapon);
                    _gameSessionService.RemoveItemFromInventory(weapon);
                    break;
                case Armor armor:
                    switch (armor)
                    {
                        case Helm helm:
                            if (_gameSessionService.Helm != null) _gameSessionService.AddItemToInventory(_gameSessionService.Helm);
                            _gameSessionService.EquipHelm(helm);
                            _gameSessionService.RemoveItemFromInventory(helm);
                            break;
                        case Chestplate chestplate:
                            if (_gameSessionService.Chestplate != null) _gameSessionService.AddItemToInventory(_gameSessionService.Chestplate);
                            _gameSessionService.EquipChestplate(chestplate);
                            _gameSessionService.RemoveItemFromInventory(chestplate);
                            break;
                    }
                    break;
            }
        }
        public void UnequipWeapon()
        {
            if (_gameSessionService.Weapon == Fists.DefaultFists) throw new EmptyException();

            _gameSessionService.AddItemToInventory(_gameSessionService.Weapon);
            _gameSessionService.RemoveWeapon();
        }
        public void UnequipHelm()
        {
            if (_gameSessionService.Helm == null) throw new EmptyException();

            _gameSessionService.AddItemToInventory(_gameSessionService.Helm);
            _gameSessionService.RemoveHelm();
        }
        public void UnequipChestplate()
        {
            if (_gameSessionService.Chestplate == null) throw new EmptyException();

            _gameSessionService.AddItemToInventory(_gameSessionService.Chestplate);
            _gameSessionService.RemoveChestplate();
        }
        public void SellInventoryItem(Item item)
        {
            if (item.Cost == null) throw new UnsellableItemException();

            _gameSessionService.RemoveItemFromInventory(item);
            _gameSessionService.AddCoins((int)item.Cost);
        }
    }
}