using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.GameObjects.Items.Equipments.Armors;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IGameSessionService _sessionService;
        public InventoryService(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public Item GetInventoryItem(int itemId)
        {
            Item? item = _sessionService.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> EquipInventoryItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Equipment equipment) throw new InvalidIdException("NOT_EQUIPMENT", "Это не снаряжение.");
            switch (equipment)
            {
                case Weapon weapon:
                    if (_sessionService.Weapon != Fists.DefaultFists) _sessionService.AddItemToInventory(_sessionService.Weapon);
                    _sessionService.EquipWeapon(weapon);
                    _sessionService.RemoveItemFromInventory(weapon);
                    break;
                case Armor armor:
                    switch (armor)
                    {
                        case Helm helm:
                            if (_sessionService.Helm != null) _sessionService.AddItemToInventory(_sessionService.Helm);
                            _sessionService.EquipHelm(helm);
                            _sessionService.RemoveItemFromInventory(helm);
                            break;
                        case Chestplate chestplate:
                            if (_sessionService.Chestplate != null) _sessionService.AddItemToInventory(_sessionService.Chestplate);
                            _sessionService.EquipChestplate(chestplate);
                            _sessionService.RemoveItemFromInventory(chestplate);
                            break;
                    }
                    break;
            }
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipWeapon()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            if (_sessionService.Weapon == Fists.DefaultFists) throw new EmptyException();
            _sessionService.AddItemToInventory(_sessionService.Weapon);
            _sessionService.RemoveWeapon();
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipHelm()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            if (_sessionService.Helm == null) throw new EmptyException();
            _sessionService.AddItemToInventory(_sessionService.Helm!);
            _sessionService.RemoveHelm();
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipChestplate()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            if (_sessionService.Chestplate == null) throw new EmptyException();
            _sessionService.AddItemToInventory(_sessionService.Chestplate!);
            _sessionService.RemoveChestplate();
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public void SellInventoryItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            if (_sessionService.CurrentRoom is not Shop) throw new NotShopException();
            Item item = GetInventoryItem(itemId);
            if (item.Cost == null) throw new UnsellableItemException();

            _sessionService.RemoveItemFromInventory(item);
            _sessionService.AddCoins((int)item.Cost);
        }
    }
}