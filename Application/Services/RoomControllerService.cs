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
        private readonly IGameSessionService _sessionService;
        private readonly IChestService _chestRepository;
        private readonly IGameInfoService _gameInfoRepository;
        private readonly IGetRoomService _getRoomByIdRepository;
        private readonly IGetItemService _getItemByIdRepository;
        private readonly IGetEnemyService _getEnemyByIdRepository;
        private readonly ICombatService _combatRepository;
        private readonly ICheckItemService _checkItemService;
        public RoomControllerService(
            IGameSessionService sessionService,
            IChestService chestRepository,
            IGameInfoService gameInfoRepository,
            IGetRoomService getRoomByIdRepository,
            IGetItemService getItemByIdRepository,
            IGetEnemyService getEnemyByIdRepository,
            ICombatService combatRepository,
            ICheckItemService checkItemService
            )
        {
            _sessionService = sessionService;
            _chestRepository = chestRepository;
            _gameInfoRepository = gameInfoRepository;
            _getRoomByIdRepository = getRoomByIdRepository;
            _getItemByIdRepository = getItemByIdRepository;
            _combatRepository = combatRepository;
            _getEnemyByIdRepository = getEnemyByIdRepository;
            _checkItemService = checkItemService;
        }
        public Room GetCurrentRoom()
        {
            RequireGameStartedAndNotStartRoom();
            return _sessionService.CurrentRoom!;
        }
        public void GoNextRoom()
        {
            RequireGameStarted();
            RequireNotInBattle();

            _sessionService.SetCurrentRoom(_sessionService.Rooms[_sessionService.CurrentRoom!.Number + 1]);
            _sessionService.CurrentRoom.Discover();

            RequireNotEndRoom();
            if (_sessionService.CurrentRoom.Enemies.Any()) _sessionService.StartBattle();
        }
        public List<Item> Search()
        {
            RequireGameStarted();
            RequireNotInBattle();

            return _sessionService.SearchCurrentRoom();
        }
        public void TakeItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            Item item = _getItemByIdRepository.GetItem(itemId, _sessionService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
            _sessionService.RemoveItemFromCurrentRoom(item);
        }
        public void TakeAllItems()
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireNotShop();

            List<Item> carryableItems = _sessionService.CurrentRoom!.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item);
                _sessionService.RemoveItemFromCurrentRoom(item);
            }
        }
        public void BuyItem(int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();
            RequireCurrentRoomIsSearched();
            RequireShop();

            Item item = _getItemByIdRepository.GetItem(itemId, _sessionService.CurrentRoom.Items);
            if (item.Cost > _sessionService.Coins) throw new NoMoneyException();

            _sessionService.AddCoins(-(int)item.Cost!);
            _sessionService.RemoveItemFromCurrentRoom(item);
            _sessionService.AddItemToInventory(item);
        }
        //public List<Enemy> GetEnemies(int roomId) => GetEnemyByIdRepository.GetEnemies();
        public Enemy GetEnemy() => _getEnemyByIdRepository.GetEnemy();

        public BattleLog DealDamage()
        {
            RequireGameStarted();
            return _combatRepository.DealDamage();

        }
        public BattleLog GetDamage()
        {
            RequireGameStarted();
            return _combatRepository.GetDamage();
        }
        public Room GetRoom(int roomId) => _getRoomByIdRepository.GetRoom(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => _gameInfoRepository.GetGameInfo();

        public BattleLog HitChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestRepository.GetChest(chestId, _sessionService.CurrentRoom!.Items);

            BattleLog battleLog;
            if (chest.Mimic is not null)
            {
                _sessionService.SetCurrentMimicChest(chest);
                _sessionService.RemoveItemFromCurrentRoom(chest);
                _sessionService.AddEnemyToCurrentRoom(chest.Mimic);
                _sessionService.StartBattle();
                battleLog = _combatRepository.DealDamage();
            }
            else
            {
                int playerHealthBeforeAttack = _sessionService.CurrentHealth;
                //attack
                var attackResult = _sessionService.Weapon.Attack(_sessionService.CurrentRoom!.Number);
                if (attackResult.SelfDamage != 0) _sessionService.AddCurrentHealth(-attackResult.SelfDamage);
                if (attackResult.IsWeaponBrokenDown) _sessionService.RemoveWeapon();

                int playerHealthAfterAttack = playerHealthBeforeAttack - _sessionService.CurrentHealth;
                battleLog = new BattleLog(ItemsLabeles.ChestName, attackResult.Damage, null, null, GeneralLabeles.PlayerName, playerHealthAfterAttack, playerHealthBeforeAttack, _sessionService.CurrentHealth);
            }
            return battleLog;
        }
        public Chest UnlockChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            if (_sessionService.Keys > 0) _sessionService.AddKeys(-1);
            else throw new NoKeyException();

            var chest = _chestRepository.GetChest(chestId, _sessionService.CurrentRoom!.Items);
            _chestRepository.UnlockChest(chest);
            return chest;
        }
        public void OpenChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestRepository.GetChest(chestId, _sessionService.CurrentRoom!.Items);
            if (_chestRepository.OpenChest(chest))
            {
                _sessionService.EndGame();
                throw new DefeatException(ExceptionLabels.PlayerEaten, _gameInfoRepository.GetGameInfo());
            }
        }
        public List<Item> SearchChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestRepository.GetChest(chestId, _sessionService.CurrentRoom!.Items);
            return _chestRepository.SearchChest(chest);
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestRepository.GetChest(chestId, _sessionService.CurrentRoom!.Items);
            var item = _getItemByIdRepository.GetItem(itemId, chest.Items);
            _chestRepository.TakeItemFromChest(chest, item);
            _checkItemService.CheckItem(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            RequireGameStarted();
            RequireNotInBattle();

            var chest = _chestRepository.GetChest(chestId, _sessionService.CurrentRoom!.Items);
            var items = _chestRepository.TakeAllItemsFromChest(chest);
            items.ForEach(_checkItemService.CheckItem);
        }
        private void RequireGameStarted()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
        }
        private void RequireNotInBattle()
        {
            if (_sessionService.IsInBattle) throw new InBattleException();
        }
        private void RequireGameStartedAndNotStartRoom()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
        }
        private void RequireNotEndRoom()
        {
            if (_sessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
        }
        private void RequireCurrentRoomIsSearched()
        {
            if (!_sessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
        }
        private void RequireNotShop()
        {
            if (_sessionService.CurrentRoom is Shop) throw new ImpossibleStealException();
        }
        private void RequireShop()
        {
            if (_sessionService.CurrentRoom is not Shop) throw new NotShopException();
        }
    }
}