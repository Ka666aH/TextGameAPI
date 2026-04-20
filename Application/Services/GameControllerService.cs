using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments;
using TextGame.Domain.GameObjects.Items.Heal;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Presentation.DTO;
using TextGame.Domain.GameExceptions;
using TextGame.Application.Interfaces.Factories;
using TextGame.Domain.GameObjects.Items.Other;
using TextGame.Domain.GameText;

namespace TextGame.Application.Services
{
    public class GameControllerService : IGameControllerService
    {
        private readonly IGameSessionService _gameSessionService;
        
        private readonly IRoomFactory _roomFactory;

        private readonly IInventoryService _inventoryRepository;
        private readonly IGameInfoService _gameInfoService;
        public GameControllerService(
            IGameSessionService gameSessionService,
            IRoomFactory roomFactory,
            IInventoryService inventoryRepository,
            IGameInfoService gameInfoService
            )
        {
            _gameSessionService = gameSessionService;
            _roomFactory = roomFactory;
            _inventoryRepository = inventoryRepository;
            _gameInfoService = gameInfoService;
        }
        public Room GetCurrentRoom()
        {
            RequireGameStartedAndNotAStartRoom();
            return _gameSessionService.CurrentRoom!;
        }
        public void Start() => _gameSessionService.StartGame();
        //public List<Room> GenerateMap()
        //{
        //    var rooms = new List<Room> { _roomFactory.CreateStartRoom() };
        //    var options = new (int Weight, Func<Room> Create)[]
        //    {
        //(GameBalance.EmptyRoomWeight, () => _roomFactory.CreateEmptyRoom(_gameSessionService)),
        //(GameBalance.SmallRoomWeight, () => _roomFactory.CreateSmallRoom(_gameSessionService)),
        //(GameBalance.BigRoomWeight, () => _roomFactory.CreateBigRoom(_gameSessionService)),
        //(GameBalance.ShopWeight, () => _roomFactory.CreateShopRoom(_gameSessionService))
        //    };
        //    int baseWeightSum = options.Sum(x => x.Weight);

        //    while (rooms.Last() is not EndRoom)
        //    {
        //        int endRoomWeight = _gameSessionService.RoomCounter;
        //        int totalWeight = baseWeightSum + endRoomWeight;

        //        int roll = Random.Shared.Next(totalWeight);

        //        if (roll >= baseWeightSum)
        //        {
        //            rooms.Add(_roomFactory.CreateEndRoom(_gameSessionService));
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
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _gameSessionService.Inventory;
        }
        public int GetCoins()
        {
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _gameSessionService.Coins;
        }
        public int GetKeys()
        {
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _gameSessionService.Keys;
        }
        public List<MapRoomDTO> GetMap()
        {
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            if (!_gameSessionService.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _gameSessionService.Rooms.Select(r => new MapRoomDTO(r.Number, r.Name ?? GeneralLabeles.GameObjectDefaultName)).ToList();
        }
        public void UseInventoryItem(int itemId)
        {
            if (!_gameSessionService.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Heal heal) throw new InvalidIdException(ExceptionLabels.NotHealCode, ExceptionLabels.NotHealText);
            _gameSessionService.RemoveItemFromInventory(heal);
            var (maxHealthBoost, currentHealthBoost) = heal.Use();
            _gameSessionService.AddMaxHealth(maxHealthBoost);
            _gameSessionService.AddCurrentHealth(currentHealthBoost);
            if (_gameSessionService.CurrentHealth <= 0) throw new DefeatException(string.Format(ExceptionLabels.PlayerPoisoned, heal.Name), GetGameInfo());
        }

        public Item GetInventoryItem(int itemId) => _inventoryRepository.GetInventoryItem(itemId);
        public List<Equipment> GetEquipment() => _inventoryRepository.GetEquipment();
        public List<Equipment> EquipInventoryItem(int itemId) => _inventoryRepository.EquipInventoryItem(itemId);
        public List<Equipment> UnequipWeapon() => _inventoryRepository.UnequipWeapon();
        public List<Equipment> UnequipHelm() => _inventoryRepository.UnequipHelm();
        public List<Equipment> UnequipChestplate() => _inventoryRepository.UnequipChestplate();
        public GameInfoDTO GetGameInfo() => _gameInfoService.GetGameInfo();

        public void SellInventoryItem(int itemId) => _inventoryRepository.SellInventoryItem(itemId);

        private void RequireGameStartedAndNotAStartRoom()
        {
            if (!_gameSessionService.IsGameStarted && _gameSessionService.Rooms.Count <= 1) throw new UnstartedGameException();
        }
    }
}