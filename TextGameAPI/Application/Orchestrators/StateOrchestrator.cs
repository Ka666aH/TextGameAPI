using TextGame.Application.DTO;
using TextGame.Application.Enums;
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

        public void BuyItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _stateService.CurrentRoom.Items);
            if (item.Cost > _stateService.Coins) throw new NoMoneyException();

            _stateService.AddCoins(-(int)item.Cost!);
            _stateService.RemoveItemFromCurrentRoom(item);
            item.RemoveStoreMargin();
            _stateService.AddItemToInventory(item);
        }

        public BattleLog DealDamage()
        {
            RequireGameStarted();
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

        public void EquipInventoryItem(int itemId)
        {
            RequireGameStarted();
            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            if (item is not Equipment equip) throw new InvalidIdException(ExceptionsLabels.NotEqiipmentCode, ExceptionsLabels.NotEqiipmentText);
            _inventoryService.EquipInventoryItem(equip);
        }

        public int GetCoins()
        {
            RequireGameStartedAndNotStartRoom();
            return _stateService.Coins;
        }

        public Room GetCurrentRoom()
        {
            RequireGameStartedAndNotStartRoom();
            return _stateService.CurrentRoom;
        }

        public BattleLog GetDamage()
        {
            RequireGameStarted();
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

        public Enemy GetEnemy()
        {
            RequireGameStarted();
            return _stateService.CurrentEnemy;
        }

        public List<Equipment> GetEquipment()
        {
            return _inventoryService.GetEquipment();
        }

        public GameInfoDTO GetGameInfo()
        {
            RequireGameStartedAndNotStartRoom();
            return _gameInfoService.GetGameInfo();
        }

        public IEnumerable<Item> GetInventory()
        {
            RequireGameStartedAndNotStartRoom();
            return _stateService.Inventory;
        }

        public Item GetInventoryItem(int itemId)
        {
            return _getItemService.GetItem(itemId, _stateService.Inventory);
        }

        public int GetKeys()
        {
            RequireGameStartedAndNotStartRoom();
            return _stateService.Keys;
        }

        public List<MapRoomDTO> GetMap()
        {
            RequireGameStartedAndNotStartRoom();

            if (!_stateService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _stateService.Rooms.Select(r => new MapRoomDTO(r.Id, r.Name ?? GeneralLabeles.GameObjectDefaultName)).ToList();
        }

        public Room GoNextRoom()
        {
            RequireGameStarted();
            RequireNotInBattle();

            _stateService.SetCurrentRoom(_stateService.CurrentRoomId + 1);
            _stateService.CurrentRoom.Discover();

            RequireNotEndRoom();
            if (_stateService.CurrentRoom.Enemy != null) _stateService.StartBattle();

            return _stateService.CurrentRoom;
        }

        public Room GoToRoom(int roomId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            _stateService.SetCurrentRoom(roomId);
            RequireNotEndRoom();

            return _stateService.CurrentRoom;
        }

        public BattleLog HitChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

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

                battleLog = new BattleLog(ItemsLabeles.ChestName, attackResult.Damage, null, null, GeneralLabeles.PlayerName, attackResult.SelfDamage, playerHealthBeforeAttack, _stateService.CurrentHealth);
            }

            return battleLog;
        }

        public void OpenChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom.Items);
            if (!_chestService.OpenChest(chest))
            {
                _stateService.EndGame();
                throw new DefeatException(ExceptionsLabels.PlayerEaten, _gameInfoService.GetGameInfo());
            }
        }

        public List<Item> Search()
        {
            RequireGameStarted();
            RequireNotInBattle();

            return _stateService.SearchCurrentRoom();
        }

        public List<Item> SearchChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            return _chestService.SearchChest(chest);
        }

        public void SellInventoryItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            _inventoryService.SellInventoryItem(item);
        }

        public void TakeAllItems()
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            List<Item> carryableItems = [.. _stateService.CurrentRoom!.Items.Where(i => i.IsCarryable)];
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item);
                _stateService.RemoveItemFromCurrentRoom(item);
            }
        }

        public void TakeAllItemsFromChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            var items = _chestService.TakeAllItemsFromChest(chest);
            items.ForEach(_checkItemService.CheckItem);
        }

        public void TakeItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            Item item = _getItemService.GetItem(itemId, _stateService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
            _stateService.RemoveItemFromCurrentRoom(item);
        }

        public void TakeItemFromChest(int chestId, int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            var item = _getItemService.GetItem(itemId, chest.Items);
            _chestService.TakeItemFromChest(chest, item);
            _checkItemService.CheckItem(item);
        }

        public void UnequipChestplate()
        {
            RequireGameStarted();
            _inventoryService.UnequipChestplate();
        }

        public void UnequipHelm()
        {
            RequireGameStarted();
            _inventoryService.UnequipHelm();
        }

        public void UnequipWeapon()
        {
            RequireGameStarted();
            _inventoryService.UnequipWeapon();
        }

        public Chest UnlockChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            if (_stateService.Keys > 0) _stateService.AddKeys(-1);
            else throw new NoKeyException();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            _chestService.UnlockChest(chest);

            return chest;
        }

        public void UseInventoryItem(int itemId)
        {
            RequireGameStarted();

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
        private void RequireGameStarted()
        {
            if (!_stateService.IsGameStarted) throw new UnstartedGameException();
        }
        private void RequireNotInBattle()
        {
            if (_stateService.IsInBattle) throw new InBattleException();
        }
        private void RequireGameStartedAndNotStartRoom()
        {
            if (!_stateService.IsGameStarted && _stateService.Rooms.Count <= 1) throw new UnstartedGameException();
        }
        private void RequireNotEndRoom()
        {
            if (_stateService.CurrentRoom is EndRoom)
            {
                _stateService.EndGame();
                throw new WinException(_gameInfoService.GetGameInfo());
            }
        }
        private void RequireCurrentRoomIsSearched()
        {
            if (!_stateService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
        }
        private void RequireNotShop()
        {
            if (_stateService.CurrentRoom is Shop) throw new ImpossibleStealException();
        }
        private void RequireShop()
        {
            if (_stateService.CurrentRoom is not Shop) throw new NotShopException();
        }
    }
}
