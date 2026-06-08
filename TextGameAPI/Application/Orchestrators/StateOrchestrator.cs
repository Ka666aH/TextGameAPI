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

        public async Task BuyItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
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

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task<BattleLog> DealDamageAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            Enemy cachedEnemy = _stateService.CurrentEnemy;
            var outcome = _combatService.DealDamage(out var battleLog);
            switch (outcome)
            {
                case DealDamageOutcome.BattleContinues: break;
                case DealDamageOutcome.EnemyDefeated:
                    if (_stateService.CurrentRoom.Enemy == null) _stateService.EndBattle();
                    await _stateService.CacheAsync(gameSessionId, ct);
                    throw new BattleWinException(string.Format(ExceptionsLabels.EnemyDefeated, cachedEnemy.Name), battleLog);
                case DealDamageOutcome.PlayerDied:
                    _stateService.EndGame();
                    await _stateService.CacheAsync(gameSessionId, ct);
                    throw new DefeatException(ExceptionsLabels.PlayerSuicideText, _gameInfoService.GetGameInfo());
            }
            await _stateService.CacheAsync(gameSessionId, ct);
            return battleLog;
        }

        public async Task EquipInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            if (item is not Equipment equip) throw new InvalidIdException(ExceptionsLabels.NotEqiipmentCode, ExceptionsLabels.NotEqiipmentText);
            _inventoryService.EquipInventoryItem(equip);

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task<int> GetCoinsAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _stateService.Coins;
        }

        public async Task<Room> GetCurrentRoomAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _stateService.CurrentRoom;
        }

        public async Task<BattleLog> GetDamageAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            Enemy cachedEnemy = _stateService.CurrentEnemy;
            var outcome = _combatService.GetDamage(out var battleLog);
            switch (outcome)
            {
                case GetDamageOutcome.BattleContinues: break;
                case GetDamageOutcome.PlayerDefeated:
                    _stateService.EndGame();
                    await _stateService.CacheAsync(gameSessionId, ct);
                    throw new DefeatException(string.Format(ExceptionsLabels.PlayerDefeated, cachedEnemy.Name), _gameInfoService.GetGameInfo());
                case GetDamageOutcome.EnemyDied:
                    _stateService.EndBattle();
                    await _stateService.CacheAsync(gameSessionId, ct);
                    throw new BattleWinException(string.Format(ExceptionsLabels.EnemySuicideText, cachedEnemy.Name), battleLog);
            }
            await _stateService.CacheAsync(gameSessionId, ct);
            return battleLog;
        }

        public async Task<Enemy> GetEnemyAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            return _stateService.CurrentEnemy;
        }

        public async Task<List<Equipment>> GetEquipmentAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            return _inventoryService.GetEquipment();
        }

        public async Task<GameInfoDTO> GetGameInfoAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _gameInfoService.GetGameInfo();
        }

        public async Task<IEnumerable<Item>> GetInventoryAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _stateService.Inventory;
        }

        public async Task<Item> GetInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            return _getItemService.GetItem(itemId, _stateService.Inventory);
        }

        public async Task<int> GetKeysAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _stateService.Keys;
        }

        public async Task<List<MapRoomDTO>> GetMapAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStartedAndNotStartRoom();

            if (!_stateService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _stateService.Rooms.Select(r => new MapRoomDTO(r.Id, r.Name ?? GeneralLabeles.GameObjectDefaultName)).ToList();
        }

        public async Task<Room> GoNextRoomAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            _stateService.SetCurrentRoom(_stateService.CurrentRoomId + 1);
            _stateService.CurrentRoom.Discover();

            RequireNotEndRoom();
            if (_stateService.CurrentRoom.Enemy != null) _stateService.StartBattle();

            await _stateService.CacheAsync(gameSessionId, ct);
            return _stateService.CurrentRoom;
        }

        public async Task<Room> GoToRoomAsync(int roomId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();
            _stateService.SetCurrentRoom(roomId);
            RequireNotEndRoom();

            await _stateService.CacheAsync(gameSessionId, ct);
            return _stateService.CurrentRoom;
        }

        public async Task<BattleLog> HitChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
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
                        await _stateService.CacheAsync(gameSessionId, ct);
                        return battleLog;
                    case DealDamageOutcome.EnemyDefeated:
                        if (_stateService.CurrentRoom.Enemy == null) _stateService.EndBattle();
                        await _stateService.CacheAsync(gameSessionId, ct);
                        throw new BattleWinException(string.Format(ExceptionsLabels.EnemyDefeated, mimic.Name), battleLog);
                    case DealDamageOutcome.PlayerDied:
                        _stateService.EndGame();
                        await _stateService.CacheAsync(gameSessionId, ct);
                        throw new DefeatException(ExceptionsLabels.PlayerSuicideText, _gameInfoService.GetGameInfo());
                }
            }
            else
            {
                int playerHealthBeforeAttack = _stateService.CurrentHealth;
                //attack
                var attackResult = _stateService.Weapon.Attack(_stateService.CurrentRoom.Id);
                if (attackResult.SelfDamage != 0) _stateService.AddCurrentHealth(-attackResult.SelfDamage);
                if (attackResult.IsWeaponBrokenDown) _stateService.RemoveWeapon();

                battleLog = new BattleLog(ItemsLabeles.ChestName, attackResult.Damage, null, null, GeneralLabeles.PlayerName, attackResult.SelfDamage, playerHealthBeforeAttack, _stateService.CurrentHealth);
            }

            await _stateService.CacheAsync(gameSessionId, ct);
            return battleLog;
        }

        public async Task OpenChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom.Items);
            if (!_chestService.OpenChest(chest))
            {
                _stateService.EndGame();
                await _stateService.CacheAsync(gameSessionId, ct);
                throw new DefeatException(ExceptionsLabels.PlayerEaten, _gameInfoService.GetGameInfo());
            }

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task<List<Item>> SearchAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var items = _stateService.SearchCurrentRoom();
            await _stateService.CacheAsync(gameSessionId, ct);
            return items;
        }

        public async Task<List<Item>> SearchChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            return _chestService.SearchChest(chest);
        }

        public async Task SellInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            RequireNotInBattle();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _stateService.Inventory);
            _inventoryService.SellInventoryItem(item);

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task TakeAllItemsAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
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

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task TakeAllItemsFromChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            var items = _chestService.TakeAllItemsFromChest(chest);
            items.ForEach(_checkItemService.CheckItem);

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task TakeItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            Item item = _getItemService.GetItem(itemId, _stateService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
            _stateService.RemoveItemFromCurrentRoom(item);

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task TakeItemFromChestAsync(int chestId, int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            var item = _getItemService.GetItem(itemId, chest.Items);
            _chestService.TakeItemFromChest(chest, item);
            _checkItemService.CheckItem(item);

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task UnequipChestplateAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryService.UnequipChestplate();

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task UnequipHelmAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryService.UnequipHelm();

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task UnequipWeaponAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

            RequireGameStarted();
            _inventoryService.UnequipWeapon();

            await _stateService.CacheAsync(gameSessionId, ct);
        }

        public async Task<Chest> UnlockChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            if (_stateService.Keys > 0) _stateService.AddKeys(-1);
            else throw new NoKeyException();

            var chest = _chestService.GetChest(chestId, _stateService.CurrentRoom!.Items);
            _chestService.UnlockChest(chest);

            await _stateService.CacheAsync(gameSessionId, ct);
            return chest;
        }

        public async Task UseInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _stateService.EnsureLoadedAsync(gameSessionId, ct);

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

            await _stateService.CacheAsync(gameSessionId, ct);
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
            if (_stateService.CurrentRoom is EndRoom) throw new WinException(_gameInfoService.GetGameInfo());
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
