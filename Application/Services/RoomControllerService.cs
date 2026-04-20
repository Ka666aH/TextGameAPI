using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Items.Other;
using TextGame.Domain.DTO;
using TextGame.Domain.GameText;

namespace TextGame.Application.Services
{
    public class RoomControllerService : IRoomControllerService
    {
        private readonly IGameSessionService _gameSessionService;
        private readonly IChestService _chestService;
        private readonly IGameInfoService _gameInfoService;
        private readonly IGetRoomService _getRoomService;
        private readonly IGetItemService _getItemService;
        private readonly IGetEnemyService _getEnemyService;
        private readonly ICombatService _combatService;
        private readonly ICheckItemService _checkItemService;
        public RoomControllerService(
            IGameSessionService gameSessionService,
            IChestService chestService,
            IGameInfoService gameInfoService,
            IGetRoomService getRoomService,
            IGetItemService getItemService,
            IGetEnemyService getEnemyService,
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
            _getEnemyService = getEnemyService;
            _checkItemService = checkItemService;
        }
        public Room GetCurrentRoom()
        {
            RequireGameStartedAndNotStartRoom();
            return _gameSessionService.CurrentRoom!;
        }
        public void GoNextRoom()
        {
            RequireGameStarted();
            RequireNotInBattle();

            _gameSessionService.SetCurrentRoom(_gameSessionService.Rooms[_gameSessionService.CurrentRoom!.Number + 1]);
            _gameSessionService.CurrentRoom.Discover();

            RequireNotEndRoom();
            if (_gameSessionService.CurrentRoom.Enemies.Any()) _gameSessionService.StartBattle();
        }
        public List<Item> Search()
        {
            RequireGameStarted();
            RequireNotInBattle();

            return _gameSessionService.SearchCurrentRoom();
        }
        public void TakeItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            Item item = _getItemService.GetItem(itemId, _gameSessionService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
            _gameSessionService.RemoveItemFromCurrentRoom(item);
        }
        public void TakeAllItems()
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            List<Item> carryableItems = _gameSessionService.CurrentRoom!.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item);
                _gameSessionService.RemoveItemFromCurrentRoom(item);
            }
        }
        public void BuyItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireShop();

            Item item = _getItemService.GetItem(itemId, _gameSessionService.CurrentRoom.Items);
            if (item.Cost > _gameSessionService.Coins) throw new NoMoneyException();

            _gameSessionService.AddCoins(-(int)item.Cost!);
            _gameSessionService.RemoveItemFromCurrentRoom(item);
            _gameSessionService.AddItemToInventory(item);
        }
        //public List<Enemy> GetEnemies(int roomId) => GetEnemyByIdRepository.GetEnemies();
        public Enemy GetEnemy() => _getEnemyService.GetEnemy();

        public BattleLog DealDamage()
        {
            RequireGameStarted();
            return _combatService.DealDamage();

        }
        public BattleLog GetDamage()
        {
            RequireGameStarted();
            return _combatService.GetDamage();
        }
        public Room GetRoom(int roomId) => _getRoomService.GetRoom(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => _gameInfoService.GetGameInfo();

        public BattleLog HitChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);

            BattleLog battleLog;
            if (chest.Mimic is not null)
            {
                _gameSessionService.SetCurrentMimicChest(chest);
                _gameSessionService.RemoveItemFromCurrentRoom(chest);
                _gameSessionService.AddEnemyToCurrentRoom(chest.Mimic);
                _gameSessionService.StartBattle();
                battleLog = _combatService.DealDamage();
            }
            else
            {
                int playerHealthBeforeAttack = _gameSessionService.CurrentHealth;
                //attack
                var attackResult = _gameSessionService.Weapon.Attack(_gameSessionService.CurrentRoom!.Number);
                if (attackResult.SelfDamage != 0) _gameSessionService.AddCurrentHealth(-attackResult.SelfDamage);
                if (attackResult.IsWeaponBrokenDown) _gameSessionService.RemoveWeapon();

                int playerHealthAfterAttack = playerHealthBeforeAttack - _gameSessionService.CurrentHealth;
                battleLog = new BattleLog(ItemsLabeles.ChestName, attackResult.Damage, null, null, GeneralLabeles.PlayerName, playerHealthAfterAttack, playerHealthBeforeAttack, _gameSessionService.CurrentHealth);
            }
            return battleLog;
        }
        public Chest UnlockChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            if (_gameSessionService.Keys > 0) _gameSessionService.AddKeys(-1);
            else throw new NoKeyException();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            _chestService.UnlockChest(chest);
            return chest;
        }
        public void OpenChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            if (_chestService.OpenChest(chest))
            {
                _gameSessionService.EndGame();
                throw new DefeatException(ExceptionLabels.PlayerEaten, _gameInfoService.GetGameInfo());
            }
        }
        public List<Item> SearchChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            return _chestService.SearchChest(chest);
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            var item = _getItemService.GetItem(itemId, chest.Items);
            _chestService.TakeItemFromChest(chest, item);
            _checkItemService.CheckItem(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestService.GetChest(chestId, _gameSessionService.CurrentRoom!.Items);
            var items = _chestService.TakeAllItemsFromChest(chest);
            items.ForEach(_checkItemService.CheckItem);
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