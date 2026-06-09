using TextGame.Application.DTO;
using TextGame.Application.Enums;
using TextGame.Application.GuardAttributes;
using TextGame.Application.Interfaces.Orchestrators;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameText;

namespace TextGame.Application.Orchestrators
{
    public class StateOrchestrator : IStateOrchestrator
    {
        private readonly IStateService _stateService;
        private readonly IGameInfoService _gameInfoService;
        private readonly IGetItemService _getItemService;

        private readonly IInventoryService _inventoryService;

        private readonly IChestService _chestService;
        private readonly ICombatService _combatService;
        private readonly ICheckItemService _checkItemService;

        public StateOrchestrator(IStateService stateService, IGameInfoService gameInfoService, IGetItemService getItemService, IInventoryService inventoryService, IChestService chestService, ICombatService combatService, ICheckItemService checkItemService)
        {
            _stateService = stateService;
            _gameInfoService = gameInfoService;
            _getItemService = getItemService;
            _inventoryService = inventoryService;
            _chestService = chestService;
            _combatService = combatService;
            _checkItemService = checkItemService;
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        [RequireCurrentRoomIsSearched]
        [RequireShop]
        public void BuyItem(int itemId)
        {
            Item item = _getItemService.GetItem(itemId, _stateService.CurrentRoom.Items);
            if (item.Cost > _stateService.Coins) throw new NoMoneyException();

            _stateService.AddCoins(-(int)item.Cost!);
            _stateService.RemoveItemFromCurrentRoom(item);
            item.RemoveStoreMargin();
            _stateService.AddItemToInventory(item);
        }

        [RequireGameStarted]
        public BattleLog DealDamage()
        {
            Enemy cachedEnemy = _stateService.CurrentEnemy;
            var outcome = _combatService.DealDamage(out var battleLog);
            switch (outcome)
            {
                case DealDamageOutcome.BattleContinues: break;
                case DealDamageOutcome.EnemyDefeated:
                    if (_stateService.CurrentRoom.Enemy == null) _stateService.EndBattle();
                    throw new BattleWinException(string.Format(ExceptionsLabels.EnemyDefeated, cachedEnemy.Name), battleLog);
                case DealDamageOutcome.PlayerDied:
                    _stateService.EndGame();
                    throw new DefeatException(ExceptionsLabels.PlayerSuicideText, _gameInfoService.GetGameInfo());
            }
            return battleLog;
        }

        [RequireGameStarted]
        public void EquipInventoryItem(int itemId)
        {
            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            if (item is not Equipment equip) throw new InvalidIdException(ExceptionsLabels.NotEquipmentCode, ExceptionsLabels.NotEquipmentText);
            _inventoryService.EquipInventoryItem(equip);
        }

        public int GetCoins() => _stateService.Coins;

        public Room GetCurrentRoom() => _stateService.CurrentRoom;

        [RequireGameStarted]
        public BattleLog GetDamage()
        {
            Enemy cachedEnemy = _stateService.CurrentEnemy;
            var outcome = _combatService.GetDamage(out var battleLog);
            switch (outcome)
            {
                case GetDamageOutcome.BattleContinues: break;
                case GetDamageOutcome.PlayerDefeated:
                    _stateService.EndGame();
                    throw new DefeatException(string.Format(ExceptionsLabels.PlayerDefeated, cachedEnemy.Name), _gameInfoService.GetGameInfo());
                case GetDamageOutcome.EnemyDied:
                    _stateService.EndBattle();
                    throw new BattleWinException(string.Format(ExceptionsLabels.EnemySuicideText, cachedEnemy.Name), battleLog);
            }
            return battleLog;
        }

        [RequireGameStarted]
        public Enemy GetEnemy() => _stateService.CurrentEnemy;

        public List<Equipment> GetEquipment() => _inventoryService.GetEquipment();

        public GameInfoDTO GetGameInfo() => _gameInfoService.GetGameInfo();

        public IEnumerable<Item> GetInventory() => _stateService.Inventory;

        public Item GetInventoryItem(int itemId) => _getItemService.GetItem(itemId, _stateService.Inventory);
        
        public int GetKeys() => _stateService.Keys;
        
        public List<MapRoomDTO> GetMap()
        {
            if (!_stateService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _stateService.Rooms.Select(r => new MapRoomDTO(r.Id, r.Name ?? GeneralLabels.GameObjectDefaultName)).ToList();
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public Room GoNextRoom()
        {
            _stateService.SetCurrentRoom(_stateService.CurrentRoomId + 1);
            _stateService.CurrentRoom.Discover();

            RequireNotEndRoom();
            if (_stateService.CurrentRoom.Enemy != null) _stateService.StartBattle();

            return _stateService.CurrentRoom;
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public Room GoToRoom(int roomId)
        {
            _stateService.SetCurrentRoom(roomId);
            RequireNotEndRoom();

            return _stateService.CurrentRoom;
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public BattleLog HitChest(int chestId)
        {
            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom.Items);
            var mimic = chest.Mimic;
            BattleLog battleLog;
            if (mimic is not null)
            {
                _stateService.SetCurrentMimicChest(chest);
                _stateService.RemoveItemFromCurrentRoom(chest);
                _stateService.AddEnemyToCurrentRoom(mimic);
                _stateService.StartBattle();
                var outcome = _combatService.DealDamage(out battleLog);
                switch (outcome)
                {
                    case DealDamageOutcome.BattleContinues:
                        return battleLog;
                    case DealDamageOutcome.EnemyDefeated:
                        if (_stateService.CurrentRoom.Enemy == null) _stateService.EndBattle();
                        throw new BattleWinException(string.Format(ExceptionsLabels.EnemyDefeated, mimic.Name), battleLog);
                    case DealDamageOutcome.PlayerDied:
                        _stateService.EndGame();
                        throw new DefeatException(ExceptionsLabels.PlayerSuicideText, _gameInfoService.GetGameInfo());
                }
            }
            else
            {
                int playerHealthBeforeAttack = _stateService.CurrentHealth;
                var attackResult = _stateService.Weapon.Attack(_stateService.CurrentRoom.Id);
                if (attackResult.SelfDamage != 0) _stateService.AddCurrentHealth(-attackResult.SelfDamage);
                if (attackResult.IsWeaponBrokenDown) _stateService.RemoveWeapon();

                battleLog = new BattleLog(ItemsLabels.ChestName, attackResult.Damage, null, null, GeneralLabels.PlayerName, attackResult.SelfDamage, playerHealthBeforeAttack, _stateService.CurrentHealth);
            }

            return battleLog;
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public void OpenChest(int chestId)
        {
            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom.Items);
            if (!_chestService.OpenChest(chest))
            {
                _stateService.EndGame();
                throw new DefeatException(ExceptionsLabels.PlayerEaten, _gameInfoService.GetGameInfo());
            }
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public List<Item> Search() =>_stateService.SearchCurrentRoom();
        
        [RequireGameStarted]
        [RequireNotInBattle]
        public List<Item> SearchChest(int chestId)
        {
            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            return _chestService.SearchChest(chest);
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        [RequireShop]
        public void SellInventoryItem(int itemId)
        {
            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            _inventoryService.SellInventoryItem(item);
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        [RequireCurrentRoomIsSearched]
        [RequireNotShop]
        public void TakeAllItems()
        {
            List<Item> carryableItems = [.. _stateService.CurrentRoom!.Items.Where(i => i.IsCarryable)];
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item);
                _stateService.RemoveItemFromCurrentRoom(item);
            }
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public void TakeAllItemsFromChest(int chestId)
        {
            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            var items = _chestService.TakeAllItemsFromChest(chest);
            items.ForEach(_checkItemService.CheckItem);
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        [RequireCurrentRoomIsSearched]
        [RequireNotShop]
        public void TakeItem(int itemId)
        {
            Item item = _getItemService.GetItem(itemId, _stateService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
            _stateService.RemoveItemFromCurrentRoom(item);
        }

        [RequireGameStarted]
        [RequireNotInBattle]
        public void TakeItemFromChest(int chestId, int itemId)
        {
            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            var item = _getItemService.GetItem(itemId, chest.Items);
            _chestService.TakeItemFromChest(chest, item);
            _checkItemService.CheckItem(item);
        }

        [RequireGameStarted]
        public void UnequipChestplate() => _inventoryService.UnequipChestplate();
        [RequireGameStarted]
        public void UnequipHelm() => _inventoryService.UnequipHelm();
        [RequireGameStarted]
        public void UnequipWeapon() => _inventoryService.UnequipWeapon();
        

        [RequireGameStarted]
        [RequireNotInBattle]
        public Chest UnlockChest(int chestId)
        {
            if (_stateService.Keys > 0) _stateService.AddKeys(-1);
            else throw new NoKeyException();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            _chestService.UnlockChest(chest);

            return chest;
        }

        [RequireGameStarted]
        public void UseInventoryItem(int itemId)
        {
            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);

            if (item is not Heal heal) throw new InvalidIdException(ExceptionsLabels.NotHealCode, ExceptionsLabels.NotHealText);

            _stateService.RemoveItemFromInventory(heal);
            var (maxHealthBoost, currentHealthBoost) = heal.Use();
            _stateService.AddMaxHealth(maxHealthBoost);
            _stateService.AddCurrentHealth(currentHealthBoost);
            if (_stateService.CurrentHealth <= 0)
                throw new DefeatException(
                    string.Format(ExceptionsLabels.PlayerPoisoned, heal.Name),
                    _gameInfoService.GetGameInfo());
        }

        private void RequireNotEndRoom()
        {
            if (_stateService.CurrentRoom is EndRoom)
            {
                _stateService.EndGame();
                throw new WinException(_gameInfoService.GetGameInfo());
            }
        }
    }
}
