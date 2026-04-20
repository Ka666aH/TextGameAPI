using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.GameObjects.Items.Equipments.Armors;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;

namespace TextGame.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IGameSessionService _gameSessionService;
        public InventoryService(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        public Item GetInventoryItem(int itemId)
        {
            Item? item = _gameSessionService.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = new List<Equipment>() { _gameSessionService.Weapon };
            if (_gameSessionService.Helm != null) equipmentList.Add(_gameSessionService.Helm);
            if (_gameSessionService.Chestplate != null) equipmentList.Add(_gameSessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> EquipInventoryItem(int itemId)
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Equipment equipment) throw new InvalidIdException(ExceptionLabels.NotEqiipmentCode, ExceptionLabels.NotEqiipmentText);
            switch (equipment)
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
            List<Equipment> equipmentList = new List<Equipment>() { _gameSessionService.Weapon };
            if (_gameSessionService.Helm != null) equipmentList.Add(_gameSessionService.Helm);
            if (_gameSessionService.Chestplate != null) equipmentList.Add(_gameSessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipWeapon()
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();

            if (_gameSessionService.Weapon == Fists.DefaultFists) throw new EmptyException();
            _gameSessionService.AddItemToInventory(_gameSessionService.Weapon);
            _gameSessionService.RemoveWeapon();
            List<Equipment> equipmentList = new List<Equipment>() { _gameSessionService.Weapon };
            if (_gameSessionService.Helm != null) equipmentList.Add(_gameSessionService.Helm);
            if (_gameSessionService.Chestplate != null) equipmentList.Add(_gameSessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipHelm()
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();

            if (_gameSessionService.Helm == null) throw new EmptyException();
            _gameSessionService.AddItemToInventory(_gameSessionService.Helm!);
            _gameSessionService.RemoveHelm();
            List<Equipment> equipmentList = new List<Equipment>() { _gameSessionService.Weapon };
            if (_gameSessionService.Helm != null) equipmentList.Add(_gameSessionService.Helm);
            if (_gameSessionService.Chestplate != null) equipmentList.Add(_gameSessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipChestplate()
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();

            if (_gameSessionService.Chestplate == null) throw new EmptyException();
            _gameSessionService.AddItemToInventory(_gameSessionService.Chestplate!);
            _gameSessionService.RemoveChestplate();
            List<Equipment> equipmentList = new List<Equipment>() { _gameSessionService.Weapon };
            if (_gameSessionService.Helm != null) equipmentList.Add(_gameSessionService.Helm);
            if (_gameSessionService.Chestplate != null) equipmentList.Add(_gameSessionService.Chestplate);
            return equipmentList;
        }
        public void SellInventoryItem(int itemId)
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();
            if (_gameSessionService.IsInBattle) throw new InBattleException();

            if (_gameSessionService.CurrentRoom is not Shop) throw new NotShopException();
            Item item = GetInventoryItem(itemId);
            if (item.Cost == null) throw new UnsellableItemException();

            _gameSessionService.RemoveItemFromInventory(item);
            _gameSessionService.AddCoins((int)item.Cost);
        }
    }
}