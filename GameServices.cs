namespace TextGame
{
    public class GameSession
    {
        public List<Room> Rooms { get; set; } = new List<Room>()
            {
                new StartRoom(),
            };
        public Room? CurrentRoom { get; set; }
        public int Coins { get; set; }
        public int Keys { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public Weapon Weapon { get; set; } = Fists.DefaultFists;
        public Helm? Helm { get; set; }
        public Chestplate? Chestplate { get; set; }
        public static readonly int DefaultMaxHealth = 10;
        public int MaxHealth { get; set; } = DefaultMaxHealth;
        public int CurrentHealth { get; set; } = DefaultMaxHealth;
        public bool IsGameStarted { get; set; }
        public void RemoveWeapon()
        {
            Weapon = Fists.DefaultFists;
        }
        public void RemoveHelm()
        {
            Helm = null;
        }
        public void RemoveChestplate()
        {
            Chestplate = null;
        }
    }
    public class GetCurrentRoomRepository : IGetCurrentRoomRepository
    {
        private GameSession Session { get; set; }
        public GetCurrentRoomRepository(GameSession session)
        {
            Session = session;
        }
        public Room GetCurrentRoom()
        {
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            return Session.CurrentRoom!;
        }
    }
    public class InventoryRepository : IInventoryRepository
    {
        private GameSession Session { get; set; }
        public InventoryRepository(GameSession session)
        {
            Session = session;
        }

        public Item GetInventoryItem(int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Item? item = Session.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
        public EquipmentListDTO GetEquipment()
        {
            return new EquipmentListDTO(Session.Weapon, Session.Helm!, Session.Chestplate!);
        }
        public EquipmentListDTO EquipInventoryItem(int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Equipment equipment) throw new InvalidIdException("NOT_EQUIPMENT", "Это не снаряжение.");
            switch (equipment)
            {
                case Weapon weapon:
                    if (Session.Weapon != Fists.DefaultFists) Session.Inventory.Add(Session.Weapon);
                    Session.Weapon = weapon;
                    Session.Inventory.Remove(weapon);
                    break;
                case Armor armor:
                    switch (armor)
                    {
                        case Helm helm:
                            if (Session.Helm != null) Session.Inventory.Add(Session.Helm);
                            Session.Helm = helm;
                            Session.Inventory.Remove(helm);
                            break;
                        case Chestplate chestplate:
                            if (Session.Chestplate != null) Session.Inventory.Add(Session.Chestplate);
                            Session.Chestplate = chestplate;
                            Session.Inventory.Remove(chestplate);
                            break;
                    }
                    break;
            }
            return new EquipmentListDTO(Session.Weapon, Session.Helm!, Session.Chestplate!);
        }
        public EquipmentListDTO UnequipWeapon()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            if (Session.Weapon == Fists.DefaultFists) throw new EmptyException();
            Session.Inventory.Add(Session.Weapon);
            Session.Weapon = Fists.DefaultFists;
            return new EquipmentListDTO(Session.Weapon, Session.Helm!, Session.Chestplate!);
        }
        public EquipmentListDTO UnequipHelm()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            if (Session.Helm == null) throw new EmptyException();
            Session.Inventory.Add(Session.Helm!);
            Session.Helm = null;
            return new EquipmentListDTO(Session.Weapon, Session.Helm!, Session.Chestplate!);
        }
        public EquipmentListDTO UnequipChestplate()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            if (Session.Chestplate == null) throw new EmptyException();
            Session.Inventory.Add(Session.Chestplate!);
            Session.Chestplate = null;
            return new EquipmentListDTO(Session.Weapon, Session.Helm!, Session.Chestplate!);
        }
    }
    public class ChestRepository : IChestRepository
    {
        private GameSession Session;
        private IGameOverStatsRepository GameOverStatsRepository;
        private IGetRoomByIdRepository GetRoomByIdRepository;
        private IGetItemByIdRepository GetItemByIdRepository;
        public ChestRepository(
            GameSession session,
            IGameOverStatsRepository gameOverStats,
            IGetRoomByIdRepository getRoomByIdRepository,
            IGetItemByIdRepository getItemByIdRepository
            )
        {
            Session = session;
            GameOverStatsRepository = gameOverStats;
            GetRoomByIdRepository = getRoomByIdRepository;
            GetItemByIdRepository = getItemByIdRepository;
        }
        public ChestDTO ReturnChestDTO(Chest chest)
        {
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public ChestDTO ReturnChestDTO(int roomId, int chestId)
        {
            Chest chest = GetChestById(roomId, chestId);
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public Chest GetChestById(int roomId, int chestId)
        {
            Room room = GetRoomByIdRepository.GetRoomById(roomId);
            Item item = GetItemByIdRepository.GetItemById(chestId, room.Items);
            if (item is not Chest) throw new InvalidIdException("NOT_CHEST", "Это не сундук.");
            return (Chest)item;
        }
        public void OpenChest(int roomId, int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsMimic)
            {
                Session.IsGameStarted = false;
                throw new DefeatException("НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!", GameOverStatsRepository.GetGameOverStats());
            }
            chest.Open();
        }
        public void UnlockChest(int roomId, int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            //Key? key = Session.Inventory!.OfType<Key>().FirstOrDefault();
            //if (key == null) throw new NoKeyException();
            //Session.Inventory.Remove(key);
            if (Session.Keys > 0) Session.Keys--;
            else throw new NoKeyException();
            chest.Unlock();
        }
        public List<Item> SearchChest(int roomId, int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsClosed) throw new ClosedException();
            return chest.Search();
        }
        public void TakeItemFromChest(int roomId, int chestId, int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = GetItemByIdRepository.GetItemById(itemId, chest.Items);
            if (item is Coin) Session.Coins++;
            else if (item is Key) Session.Keys++;
            else Session.Inventory.Add(item);
            chest.Items.Remove(item);
        }
        public void TakeAllItemsFromChest(int roomId, int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            List<Item> carryableItems = chest.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                if (item is Coin) Session.Coins++;
                else if (item is Key) Session.Keys++;
                else Session.Inventory.Add(item);
            }
            chest.Items.RemoveAll(x => x.IsCarryable);
        }
    }
    //public class InventoryRepository : IInventoryRepository
    //{
    //    private GameSession Session;
    //    private IGetItemByIdRepository GetItemByIdRepository;
    //    public InventoryRepository(
    //        GameSession session,
    //        IGetItemByIdRepository getItemByIdRepository
    //        )
    //    {
    //        Session = session;
    //        GetItemByIdRepository = getItemByIdRepository;
    //    }
    //    public Item GetInventoryItem(int itemId)
    //    {
    //        return GetItemByIdRepository.GetItemById(itemId, Session.Inventory);
    //    }
    //    public List<Item> GetInventoryItems(List<int> itemsIds)
    //    {
    //        List<Item> items = new List<Item>();
    //        foreach (var itemId in itemsIds)
    //        {
    //            items.Add(GetInventoryItem(itemId));
    //        }
    //        return items;
    //    }
    //}
    public class GameStatsRepository : IGameStatsRepository
    {
        private GameSession Session;
        public GameStatsRepository(GameSession session)
        {
            Session = session;
        }
        public GameStatsDTO GetGameStats()
        {
            return new GameStatsDTO(Session.Coins, Session.Keys, GameObjectMapper.ToDTO(Session.Inventory));
        }
    }
    public class GameOverStatsRepository : IGameOverStatsRepository
    {
        private GameSession Session;
        public GameOverStatsRepository(GameSession session)
        {
            Session = session;
        }
        public GameOverStatsDTO GetGameOverStats()
        {
            return new GameOverStatsDTO(Session.CurrentRoom!.Number, Session.Coins, Session.Keys, GameObjectMapper.ToDTO(Session.Inventory));
        }
    }
    public class GetRoomByIdRepository : IGetRoomByIdRepository
    {
        private GameSession Session;
        IGameOverStatsRepository GameOverStatsRepository;
        public GetRoomByIdRepository(
            GameSession session,
            IGameOverStatsRepository gameOverStatsRepository
            )
        {
            Session = session;
            GameOverStatsRepository = gameOverStatsRepository;
        }
        public Room GetRoomById(int roomId)
        {
            /*//Старый вариант
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");*/
            if (roomId < 0 || roomId > Session.Rooms.Count) throw new NullRoomIdException();
            Room room = Session.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            Session.CurrentRoom = room;
            if (Session.CurrentRoom is EndRoom) throw new WinException(GameOverStatsRepository.GetGameOverStats());
            return room;
        }
    }
    public class GetItemByIdRepository : IGetItemByIdRepository
    {
        public Item GetItemById(int itemId, List<Item> items)
        {
            Item? item = items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
    }
    public class GameControllerRepository : IGameControllerRepository
    {
        private GameSession Session;
        private IGetCurrentRoomRepository GetCurrentRoomRepository;

        private readonly IRoomNumberFactory RoomNumberFactory;
        private readonly IRoomFactory RoomFactory;
        private readonly IItemIdFactory ItemIdFactory;

        private readonly IInventoryRepository InventoryRepository;
        public GameControllerRepository(
            GameSession session,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IRoomNumberFactory roomNumberFactory,
            IRoomFactory roomFactory,
            IItemIdFactory itemIdFactory,
            IInventoryRepository inventoryRepository
            )
        {
            Session = session;
            GetCurrentRoomRepository = getCurrentRoomRepository;
            RoomNumberFactory = roomNumberFactory;
            RoomFactory = roomFactory;
            ItemIdFactory = itemIdFactory;
            InventoryRepository = inventoryRepository;
        }
        public Room GetCurrentRoom() => GetCurrentRoomRepository.GetCurrentRoom();
        public void Start()
        {
            ResetGame();
            Session.Rooms = GenerateMap();
            Session.CurrentRoom = Session.Rooms[0];
            Session.IsGameStarted = true;
        }
        public void ResetGame()
        {
            Session.Coins = 0;
            Session.Keys = 0;

            Session.Weapon = Fists.DefaultFists;
            Session.Helm = null;
            Session.Chestplate = null;

            Session.Inventory = new List<Item>();
            Session.Rooms = new List<Room>();

            Session.MaxHealth = GameSession.DefaultMaxHealth;
            Session.CurrentHealth = GameSession.DefaultMaxHealth;

            ItemIdFactory.Reset();
            RoomNumberFactory.Reset();
        }
        public List<Room> GenerateMap()
        {
            List<Room> rooms = new List<Room>()
            {
                new StartRoom(),
            };
            while (rooms.Last() is not EndRoom)
            {
                rooms.Add(RoomFactory.CreateRoom());
            }
            return rooms;
        }
        public List<Item> GetInventory()
        {
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            return Session.Inventory;
        }
        public int GetCoins()
        {
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            return Session.Coins;
        }
        public int GetKeys()
        {
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            return Session.Keys;
        }
        public List<MapRoomDTO> GetMap()
        {
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            if (!Session.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return Session.Rooms.Select(r => new MapRoomDTO(r.Number, r.Name ?? "НЕИЗВЕСТНО")).ToList();
        }

        public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        public EquipmentListDTO GetEquipment() => InventoryRepository.GetEquipment();
        public EquipmentListDTO EquipInventoryItem(int itemId) => InventoryRepository.EquipInventoryItem(itemId);
        public EquipmentListDTO UnequipWeapon() => InventoryRepository.UnequipWeapon();
        public EquipmentListDTO UnequipHelm() => InventoryRepository.UnequipHelm();
        public EquipmentListDTO UnequipChestplate() => InventoryRepository.UnequipChestplate();

    }
    public class RoomControllerRepository : IRoomControllerRepository
    {
        private GameSession Session;
        private IGetCurrentRoomRepository GetCurrentRoomRepository;
        private IChestRepository ChestRepository;
        private IGameStatsRepository GameStatsRepository;
        private IGetRoomByIdRepository GetRoomByIdRepository;
        private IGetItemByIdRepository GetItemByIdRepository;
        private IGameOverStatsRepository GameOverStatsRepository;
        public RoomControllerRepository(
            GameSession session,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IChestRepository chestRepository,
            IGameStatsRepository gameStatsRepository,
            IGetRoomByIdRepository getRoomByIdRepository,
            IGetItemByIdRepository getItemByIdRepository,
            IGameOverStatsRepository getGameOverStatsRepository
            )
        {
            Session = session;
            GetCurrentRoomRepository = getCurrentRoomRepository;
            ChestRepository = chestRepository;
            GameStatsRepository = gameStatsRepository;
            GetRoomByIdRepository = getRoomByIdRepository;
            GetItemByIdRepository = getItemByIdRepository;
            GameOverStatsRepository = getGameOverStatsRepository;
        }
        public Room GetCurrentRoom() => GetCurrentRoomRepository.GetCurrentRoom();
        public void GoNextRoom()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            Session.CurrentRoom = Session.Rooms[Session.CurrentRoom!.Number + 1];
            Session.CurrentRoom.IsDiscovered = true;
            if (Session.CurrentRoom is EndRoom) throw new WinException(GameOverStatsRepository.GetGameOverStats());
        }
        public List<Item> Search(int roomId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Room room = GetRoomById(roomId);
            return room!.Items;
        }
        public void TakeItem(int roomId, int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            Room room = GetRoomById(roomId);
            Item item = GetItemByIdRepository.GetItemById(itemId, Session.CurrentRoom!.Items);
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is Coin) Session.Coins++;
            else if (item is Key) Session.Keys++;
            else Session.Inventory.Add(item);
            Session.CurrentRoom!.Items.Remove(item);
        }
        public void TakeAllItems(int roomId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            Room room = GetRoomById(roomId);
            List<Item> carryableItems = room.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                if (!item.IsCarryable) continue;
                if (item is Coin) Session.Coins++;
                else if (item is Key) Session.Keys++;
                else Session.Inventory.Add(item);
            }
            room.Items.RemoveAll(x => x.IsCarryable);
        }
        public ChestDTO ReturnChestDTO(Chest chest) => ChestRepository.ReturnChestDTO(chest);
        public ChestDTO ReturnChestDTO(int roomId, int chestId) => ChestRepository.ReturnChestDTO(roomId, chestId);
        public void OpenChest(int roomId, int chestId) => ChestRepository.OpenChest(roomId, chestId);
        public void UnlockChest(int roomId, int chestId) => ChestRepository.UnlockChest(roomId, chestId);
        public List<Item> SearchChest(int roomId, int chestId) => ChestRepository.SearchChest(roomId, chestId);
        public void TakeItemFromChest(int roomId, int chestId, int itemId) => ChestRepository.TakeItemFromChest(roomId, chestId, itemId);
        public void TakeAllItemsFromChest(int roomId, int chestId) => ChestRepository.TakeAllItemsFromChest(roomId, chestId);
        public Room GetRoomById(int roomId) => GetRoomByIdRepository.GetRoomById(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameStatsDTO GetGameStats() => GameStatsRepository.GetGameStats();
    }
}