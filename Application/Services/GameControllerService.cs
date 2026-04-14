using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;
using TextGame.Domain.GameObjects.Items.Heal;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameExceptions;
using TextGame.Application.Interfaces.Factories;

namespace TextGame.Application.Services
{
    public class GameControllerService : IGameControllerService
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGetCurrentRoomService _getCurrentRoomRepository;

        private readonly IRoomFactory _roomFactory;

        private readonly IInventoryService _inventoryRepository;
        private readonly IGameInfoService _gameInfoRepository;
        public GameControllerService(
            IGameSessionService sessionService,
            IGetCurrentRoomService getCurrentRoomRepository,
            IRoomFactory roomFactory,
            IInventoryService inventoryRepository,
            IGameInfoService gameInfoRepository
            )
        {
            _sessionService = sessionService;
            _getCurrentRoomRepository = getCurrentRoomRepository;
            _roomFactory = roomFactory;
            _inventoryRepository = inventoryRepository;
            _gameInfoRepository = gameInfoRepository;
        }
        public Room GetCurrentRoom() => _getCurrentRoomRepository.GetCurrentRoom();
        public void Start() => _sessionService.StartGame();
        //public List<Room> GenerateMap()
        //{
        //    var rooms = new List<Room> { _roomFactory.CreateStartRoom() };
        //    var options = new (int Weight, Func<Room> Create)[]
        //    {
        //(GameBalance.EmptyRoomWeight, () => _roomFactory.CreateEmptyRoom(_sessionService)),
        //(GameBalance.SmallRoomWeight, () => _roomFactory.CreateSmallRoom(_sessionService)),
        //(GameBalance.BigRoomWeight, () => _roomFactory.CreateBigRoom(_sessionService)),
        //(GameBalance.ShopWeight, () => _roomFactory.CreateShopRoom(_sessionService))
        //    };
        //    int baseWeightSum = options.Sum(x => x.Weight);

        //    while (rooms.Last() is not EndRoom)
        //    {
        //        int endRoomWeight = _sessionService.RoomCounter;
        //        int totalWeight = baseWeightSum + endRoomWeight;

        //        int roll = Random.Shared.Next(totalWeight);

        //        if (roll >= baseWeightSum)
        //        {
        //            rooms.Add(_roomFactory.CreateEndRoom(_sessionService));
        //        }
        //        else
        //        {
        //            int acc = 0;
        //            foreach (var (weight, create) in options)
        //            {
        //                if (roll < acc + weight)
        //                {
        //                    rooms.Add(create());
        //                    break;
        //                }
        //                acc += weight;
        //            }
        //        }
        //    }

        //    return rooms;
        //}
        public IEnumerable<Item> GetInventory()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _sessionService.Inventory;
        }
        public int GetCoins()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _sessionService.Coins;
        }
        public int GetKeys()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _sessionService.Keys;
        }
        public List<MapRoomDTO> GetMap()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            if (!_sessionService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _sessionService.Rooms.Select(r => new MapRoomDTO(r.Number, r.Name ?? "НЕИЗВЕСТНО")).ToList();
        }
        public void UseInventoryItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Heal heal) throw new InvalidIdException("NOT_HEAL", "Это не предмет лечения.");
            _sessionService.RemoveItemFromInventory(heal);
            var (maxHealthBoost, currentHealthBoost) = heal.Use();
            _sessionService.AddMaxHealth(maxHealthBoost);
            _sessionService.AddCurrentHealth(currentHealthBoost);
            if (_sessionService.CurrentHealth <= 0) throw new DefeatException($"{heal.Name} приводит Вас к гибели!", GetGameInfo());
        }

        public Item GetInventoryItem(int itemId) => _inventoryRepository.GetInventoryItem(itemId);
        public List<Equipment> GetEquipment() => _inventoryRepository.GetEquipment();
        public List<Equipment> EquipInventoryItem(int itemId) => _inventoryRepository.EquipInventoryItem(itemId);
        public List<Equipment> UnequipWeapon() => _inventoryRepository.UnequipWeapon();
        public List<Equipment> UnequipHelm() => _inventoryRepository.UnequipHelm();
        public List<Equipment> UnequipChestplate() => _inventoryRepository.UnequipChestplate();
        public GameInfoDTO GetGameInfo() => _gameInfoRepository.GetGameInfo();

        public void SellInventoryItem(int itemId) => _inventoryRepository.SellInventoryItem(itemId);
    }
}