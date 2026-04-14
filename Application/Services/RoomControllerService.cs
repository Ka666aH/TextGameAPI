using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class RoomControllerService : IRoomControllerService
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGetCurrentRoomService _getCurrentRoomRepository;
        private readonly IChestService _chestRepository;
        private readonly IGameInfoService _gameInfoRepository;
        private readonly IGetRoomService _getRoomByIdRepository;
        private readonly IGetItemService _getItemByIdRepository;
        private readonly IGetEnemyService _getEnemyByIdRepository;
        private readonly ICombatService _combatRepository;
        private readonly ICheckItemService _checkItemService;
        public RoomControllerService(
            IGameSessionService sessionService,
            IGetCurrentRoomService getCurrentRoomRepository,
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
            _getCurrentRoomRepository = getCurrentRoomRepository;
            _chestRepository = chestRepository;
            _gameInfoRepository = gameInfoRepository;
            _getRoomByIdRepository = getRoomByIdRepository;
            _getItemByIdRepository = getItemByIdRepository;
            _combatRepository = combatRepository;
            _getEnemyByIdRepository = getEnemyByIdRepository;
            _checkItemService = checkItemService;
        }
        public Room GetCurrentRoom() => _getCurrentRoomRepository.GetCurrentRoom();
        public void GoNextRoom()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            _sessionService.SetCurrentRoom(_sessionService.Rooms[_sessionService.CurrentRoom!.Number + 1]);
            _sessionService.CurrentRoom.Discover();
            if (_sessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
            _sessionService.StartBattle();
        }
        public IEnumerable<Item> Search()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            return _sessionService.SearchCurrentRoom();
        }
        public void TakeItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();
            if (!_sessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
            if (_sessionService.CurrentRoom is Shop) throw new ImpossibleStealException();

            //Room room = GetRoomById(roomId);
            //Room room = Session.CurrentRoom!;
            Item item = _getItemByIdRepository.GetItem(itemId, _sessionService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item, _sessionService);
            _sessionService.RemoveItemFromCurrentRoom(item);
        }
        public void TakeAllItems()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();
            if (!_sessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
            if (_sessionService.CurrentRoom is Shop) throw new ImpossibleStealException();

            //Room room = GetRoomById(roomId);
            List<Item> carryableItems = _sessionService.CurrentRoom!.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item, _sessionService);
                _sessionService.RemoveItemFromCurrentRoom(item);
            }
        }
        public void BuyItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();
            if (!_sessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();

            if (_sessionService.CurrentRoom is not Shop) throw new NotShopException();
            Item item = _getItemByIdRepository.GetItem(itemId, _sessionService.CurrentRoom.Items);
            if (item.Cost > _sessionService.Coins) throw new NoMoneyException();

            _sessionService.AddCoins(-(int)item.Cost!);
            _sessionService.RemoveItemFromCurrentRoom(item);
            _sessionService.AddItemToInventory(item);
        }
        //public List<Enemy> GetEnemies(int roomId) => GetEnemyByIdRepository.GetEnemies();
        public Enemy GetEnemyById() => _getEnemyByIdRepository.GetEnemyById();

        public BattleLog DealDamage() => _combatRepository.DealDamage();
        public BattleLog GetDamage() => _combatRepository.GetDamage();

        public ChestStateDTO ReturnChestDTO(Chest chest) => _chestRepository.ReturnChestDTO(chest);
        public ChestStateDTO ReturnChestDTO(int chestId) => _chestRepository.ReturnChestDTO(chestId);
        public BattleLog HitChest(int chestId) => _chestRepository.HitChest(chestId);
        public void OpenChest(int chestId) => _chestRepository.OpenChest(chestId);
        public void UnlockChest(int chestId) => _chestRepository.UnlockChest(chestId);
        public IEnumerable<Item> SearchChest(int chestId) => _chestRepository.SearchChest(chestId);
        public void TakeItemFromChest(int chestId, int itemId) => _chestRepository.TakeItemFromChest(chestId, itemId);
        public void TakeAllItemsFromChest(int chestId) => _chestRepository.TakeAllItemsFromChest(chestId);
        public Room GetRoom(int roomId) => _getRoomByIdRepository.GetRoom(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => _gameInfoRepository.GetGameInfo();
    }
}