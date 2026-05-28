using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.DTO;
using TextGame.Domain.GameText;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Application.DTO;
using TextGame.Application.Enums;

namespace TextGame.Application.Services
{
    public class RoomControllerService : IRoomControllerService
    {
        private readonly IGameSessionStateService _gameSessionService;
        private readonly IChestService _chestService;
        private readonly IGameInfoService _gameInfoService;
        private readonly IGetRoomService _getRoomService;
        private readonly IGetItemService _getItemService;
        private readonly ICombatService _combatService;
        private readonly ICheckItemService _checkItemService;
        public RoomControllerService(
            IGameSessionStateService gameSessionService,
            IChestService chestService,
            IGameInfoService gameInfoService,
            IGetRoomService getRoomService,
            IGetItemService getItemService,
            ICombatService combatService,
            ICheckItemService checkItemService
            )
        {
            _gameSessionService = gameSessionService;
            _chestService = chestService;
            _gameInfoService = gameInfoService;
            _getRoomService = getRoomService;
            _getItemService = getItemService;
            _combatService = combatService;
            _checkItemService = checkItemService;
        }
        public async Task<Room> GetCurrentRoomAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _gameSessionService.CurrentRoom;
        }
        public async Task<Room> GoNextRoomAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            _gameSessionService.SetCurrentRoom(_gameSessionService.CurrentRoomId + 1);
            _gameSessionService.CurrentRoom.Discover();

            RequireNotEndRoom();
            if (_gameSessionService.CurrentRoom.Enemy != null) _gameSessionService.StartBattle();

            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return _gameSessionService.CurrentRoom;
        }
        public async Task<Room> GoToRoomAsync(int roomId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();
            _gameSessionService.SetCurrentRoom(roomId);
            RequireNotEndRoom();

            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return _gameSessionService.CurrentRoom;
        }
        public async Task<List<Item>> SearchAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var items = _gameSessionService.SearchCurrentRoom();
            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return items;
        }
        public async Task TakeItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            Item item = _getItemService.GetItem(itemId, _gameSessionService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
            _gameSessionService.RemoveItemFromCurrentRoom(item);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task TakeAllItemsAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            List<Item> carryableItems = [.. _gameSessionService.CurrentRoom!.Items.Where(i => i.IsCarryable)];
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item);
                _gameSessionService.RemoveItemFromCurrentRoom(item);
            }

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task BuyItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _gameSessionService.CurrentRoom.Items);
            if (item.Cost > _gameSessionService.Coins) throw new NoMoneyException();

            _gameSessionService.AddCoins(-(int)item.Cost!);
            _gameSessionService.RemoveItemFromCurrentRoom(item);
            item.RemoveStoreMargin();
            _gameSessionService.AddItemToInventory(item);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task<Enemy> GetEnemyAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            return _gameSessionService.CurrentEnemy;
        }
        public async Task<BattleLog> DealDamageAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            Enemy cachedEnemy = _gameSessionService.CurrentEnemy;
            var outcome = _combatService.DealDamage(out var battleLog);
            switch(outcome)
            {
                case DealDamageOutcome.BattleContinues: break;
                case DealDamageOutcome.EnemyDefeated:
                    if (_gameSessionService.CurrentRoom.Enemy == null) _gameSessionService.EndBattle();
                    await _gameSessionService.CacheAsync(gameSessionId, ct);
                    throw new BattleWinException(string.Format(ExceptionsLabels.EnemyDefeated, cachedEnemy.Name), battleLog);
                case DealDamageOutcome.PlayerDied:
                    _gameSessionService.EndGame();
                    await _gameSessionService.CacheAsync(gameSessionId, ct);
                    throw new DefeatException(ExceptionsLabels.PlayerSuicideText, _gameInfoService.GetGameInfo());
            }
            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return battleLog;
        }
        public async Task<BattleLog> GetDamageAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            Enemy cachedEnemy = _gameSessionService.CurrentEnemy;
            var outcome = _combatService.GetDamage(out var battleLog);
            switch (outcome)
            {
                case GetDamageOutcome.BattleContinues: break;
                case GetDamageOutcome.PlayerDefeated:
                    _gameSessionService.EndGame();
                    await _gameSessionService.CacheAsync(gameSessionId, ct);
                    throw new DefeatException(string.Format(ExceptionsLabels.PlayerDefeated, cachedEnemy.Name), _gameInfoService.GetGameInfo());
                case GetDamageOutcome.EnemyDied:
                    _gameSessionService.EndBattle();
                    await _gameSessionService.CacheAsync(gameSessionId, ct);
                    throw new BattleWinException(string.Format(ExceptionsLabels.EnemySuicideText, cachedEnemy.Name), battleLog);
            }
            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return battleLog;
        }
        public async Task<GameInfoDTO> GetGameInfoAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStartedAndNotStartRoom();
            return _gameInfoService.GetGameInfo();
        }
        public async Task<BattleLog> HitChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom.Items);
            var mimic = chest.Mimic;
            BattleLog battleLog;
            if (mimic is not null)
            {
                _gameSessionService.SetCurrentMimicChest(chest);
                _gameSessionService.RemoveItemFromCurrentRoom(chest);
                _gameSessionService.AddEnemyToCurrentRoom(mimic);
                _gameSessionService.StartBattle();
                var outcome = _combatService.DealDamage(out battleLog);
                switch (outcome)
                {
                    case DealDamageOutcome.BattleContinues:
                        await _gameSessionService.CacheAsync(gameSessionId, ct);
                        return battleLog;
                    case DealDamageOutcome.EnemyDefeated:
                        if (_gameSessionService.CurrentRoom.Enemy == null) _gameSessionService.EndBattle();
                        await _gameSessionService.CacheAsync(gameSessionId, ct);
                        throw new BattleWinException(string.Format(ExceptionsLabels.EnemyDefeated, mimic.Name), battleLog);
                    case DealDamageOutcome.PlayerDied:
                        _gameSessionService.EndGame();
                        await _gameSessionService.CacheAsync(gameSessionId, ct);
                        throw new DefeatException(ExceptionsLabels.PlayerSuicideText, _gameInfoService.GetGameInfo());
                }
            }
            else
            {
                int playerHealthBeforeAttack = _gameSessionService.CurrentHealth;
                //attack
                var attackResult = _gameSessionService.Weapon.Attack(_gameSessionService.CurrentRoom.Id);
                if (attackResult.SelfDamage != 0) _gameSessionService.AddCurrentHealth(-attackResult.SelfDamage);
                if (attackResult.IsWeaponBrokenDown) _gameSessionService.RemoveWeapon();

                battleLog = new BattleLog(ItemsLabeles.ChestName, attackResult.Damage, null, null, GeneralLabeles.PlayerName, attackResult.SelfDamage, playerHealthBeforeAttack, _gameSessionService.CurrentHealth);
            }

            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return battleLog;
        }
        public async Task<Chest> UnlockChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            if (_gameSessionService.Keys > 0) _gameSessionService.AddKeys(-1);
            else throw new NoKeyException();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            _chestService.UnlockChest(chest);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
            return chest;
        }
        public async Task OpenChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom.Items);
            if (!_chestService.OpenChest(chest))
            {
                _gameSessionService.EndGame();
                await _gameSessionService.CacheAsync(gameSessionId, ct);
                throw new DefeatException(ExceptionsLabels.PlayerEaten, _gameInfoService.GetGameInfo());
            }

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task<List<Item>> SearchChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            return _chestService.SearchChest(chest);
        }
        public async Task TakeItemFromChestAsync(int chestId, int itemId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            var item = _getItemService.GetItem(itemId, chest.Items);
            _chestService.TakeItemFromChest(chest, item);
            _checkItemService.CheckItem(item);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        public async Task TakeAllItemsFromChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default)
        {
            await _gameSessionService.EnsureLoadedAsync(gameSessionId, ct);
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            var items = _chestService.TakeAllItemsFromChest(chest);
            items.ForEach(_checkItemService.CheckItem);

            await _gameSessionService.CacheAsync(gameSessionId, ct);
        }
        private void RequireGameStarted()
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();
        }
        private void RequireNotInBattle()
        {
            if (_gameSessionService.IsInBattle) throw new InBattleException();
        }
        private void RequireGameStartedAndNotStartRoom()
        {
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
        }
        private void RequireNotEndRoom()
        {
            if (_gameSessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoService.GetGameInfo());
        }
        private void RequireCurrentRoomIsSearched()
        {
            if (!_gameSessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
        }
        private void RequireNotShop()
        {
            if (_gameSessionService.CurrentRoom is Shop) throw new ImpossibleStealException();
        }
        private void RequireShop()
        {
            if (_gameSessionService.CurrentRoom is not Shop) throw new NotShopException();
        }
    }
}