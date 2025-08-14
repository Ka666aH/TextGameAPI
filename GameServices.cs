namespace TextGame
{
    public class GameSession
    {
        public List<Room> Rooms { get; set; } = new List<Room>()
            {
                new StartRoom(),
            };
        public Room? CurrentRoom { get; set; }
        public Weapon Weapon { get; set; } = Fists.DefaultFists;
        public Helm? Helm { get; set; }
        public Chestplate? Chestplate { get; set; }
        public static readonly int DefaultMaxHealth = 10;
        public int MaxHealth { get; set; } = DefaultMaxHealth;
        public int CurrentHealth { get; set; } = DefaultMaxHealth;
        public int Coins { get; set; }
        public int Keys { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public bool IsGameStarted { get; set; }
        public bool IsInBattle { get; set; } = false;
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
    public class CombatRepository : ICombatRepository
    {
        private GameSession Session;
        private IGetEnemyByIdRepository GetEnemyByIdRepository;
        private IGameInfoRepository GameInfoRepository;
        public CombatRepository(
            GameSession gameSession,
            IGetEnemyByIdRepository getEnemyByIdRepository,
            IGameInfoRepository gameInfoRepository
            )
        {
            Session = gameSession;
            GetEnemyByIdRepository = getEnemyByIdRepository;
            GameInfoRepository = gameInfoRepository;
        }
        public BattleLog DealDamage()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            int playerHealthBeforeAttack = Session.CurrentHealth;
            Enemy enemy = GetEnemyByIdRepository.GetEnemyById();
            int damage = Session.Weapon.Attack(Session);
            int enemyHealthBeforeAttack = enemy.Health;
            int enemyHealthAfterAttack = enemy.GetDamage(damage, Session.CurrentRoom!);
            BattleLog battleLog = new BattleLog(enemy.Name!, damage, enemyHealthBeforeAttack, enemyHealthAfterAttack, "ИГРОК", playerHealthBeforeAttack, Session.CurrentHealth);
            if (enemyHealthAfterAttack <= 0)
            {
                Session.CurrentRoom!.Enemies.Remove(enemy);
                if (Session.CurrentHealth <= 0) throw new DefeatException("Вы погибли от своей же атаки. Как отчаянно.", GameInfoRepository.GetGameInfo());//дубль
                if (!Session.CurrentRoom.Enemies.Any()) Session.IsInBattle = false;
                throw new BattleWinException($"{enemy.Name!} повержен.", battleLog);
            }
            if (Session.CurrentHealth <= 0) throw new DefeatException("Вы погибли от своей же атаки. Как отчаянно.", GameInfoRepository.GetGameInfo()); //дубль
            return battleLog;
        }
        public BattleLog GetDamage()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Enemy enemy = GetEnemyByIdRepository.GetEnemyById();
            int enemyHealthBeforeAttack = enemy.Health;
            int damage = enemy.Attack();
            int helmBlock = Session.Helm != null ? Session.Helm.Block(Session) : 0;
            int chestplateBlock = Session.Chestplate != null ? Session.Chestplate.Block(Session) : 0;
            int damageAfterBlock = damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = Session.CurrentHealth;
            if (damageAfterBlock > 0) Session.CurrentHealth -= damageAfterBlock;
            if (Session.CurrentHealth <= 0) throw new DefeatException($"Вы были повержены {enemy.Name}ОМ.", GameInfoRepository.GetGameInfo());

            return new BattleLog("ИГРОК", damage, playerHealthBeforeAttack, Session.CurrentHealth, enemy.Name!, enemyHealthBeforeAttack, enemy.Health);
        }
    }
    public class GetEnemyByIdRepository : IGetEnemyByIdRepository
    {
        private GameSession Session;
        private IGetRoomByIdRepository GetRoomByIdRepository;
        public GetEnemyByIdRepository(
            IGetRoomByIdRepository getRoomByIdRepository,
            GameSession session)
        {
            GetRoomByIdRepository = getRoomByIdRepository;
            Session = session;
        }
        //public List<Enemy> GetEnemies()
        //{
        //    if (!Session.IsGameStarted) throw new UnstartedGameException();

        //    Room room = Session.CurrentRoom!;
        //    return room.Enemies;
        //}
        public Enemy GetEnemyById()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Room room = Session.CurrentRoom!;
            //Enemy? enemy = room.Enemies.FirstOrDefault(e => e.Id == enemyId);
            Enemy? enemy = room.Enemies.FirstOrDefault();
            if (enemy == null) throw new NullEnemyIdException();
            return enemy;
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
            Item? item = Session.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = new List<Equipment>() { Session.Weapon };
            if (Session.Helm != null) equipmentList.Add(Session.Helm);
            if (Session.Chestplate != null) equipmentList.Add(Session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> EquipInventoryItem(int itemId)
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
            List<Equipment> equipmentList = new List<Equipment>() { Session.Weapon };
            if (Session.Helm != null) equipmentList.Add(Session.Helm);
            if (Session.Chestplate != null) equipmentList.Add(Session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipWeapon()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            if (Session.Weapon == Fists.DefaultFists) throw new EmptyException();
            Session.Inventory.Add(Session.Weapon);
            Session.Weapon = Fists.DefaultFists;
            List<Equipment> equipmentList = new List<Equipment>() { Session.Weapon };
            if (Session.Helm != null) equipmentList.Add(Session.Helm);
            if (Session.Chestplate != null) equipmentList.Add(Session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipHelm()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            if (Session.Helm == null) throw new EmptyException();
            Session.Inventory.Add(Session.Helm!);
            Session.Helm = null;
            List<Equipment> equipmentList = new List<Equipment>() { Session.Weapon };
            if (Session.Helm != null) equipmentList.Add(Session.Helm);
            if (Session.Chestplate != null) equipmentList.Add(Session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipChestplate()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            if (Session.Chestplate == null) throw new EmptyException();
            Session.Inventory.Add(Session.Chestplate!);
            Session.Chestplate = null;
            List<Equipment> equipmentList = new List<Equipment>() { Session.Weapon };
            if (Session.Helm != null) equipmentList.Add(Session.Helm);
            if (Session.Chestplate != null) equipmentList.Add(Session.Chestplate);
            return equipmentList;
        }
    }
    public class ChestRepository : IChestRepository
    {
        private GameSession Session;

        private IGameInfoRepository GameInfoRepository;
        private IGetRoomByIdRepository GetRoomByIdRepository;
        private IGetItemByIdRepository GetItemByIdRepository;
        private ICombatRepository CombatRepository;

        private IEnemyFactory EnemyFactory;
        public ChestRepository(
            GameSession session,
            IGameInfoRepository gameInfoRepository,
            IGetRoomByIdRepository getRoomByIdRepository,
            IGetItemByIdRepository getItemByIdRepository,
            IEnemyFactory enemyFactory,
            ICombatRepository combatRepository
            )
        {
            Session = session;
            GameInfoRepository = gameInfoRepository;
            GetRoomByIdRepository = getRoomByIdRepository;
            GetItemByIdRepository = getItemByIdRepository;
            EnemyFactory = enemyFactory;
            CombatRepository = combatRepository;
        }
        public ChestDTO ReturnChestDTO(Chest chest)
        {
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public ChestDTO ReturnChestDTO(int chestId)
        {
            Chest chest = GetChestById(chestId);
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public Chest GetChestById(int chestId)
        {
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);
            Item item = GetItemByIdRepository.GetItemById(chestId, Session.CurrentRoom!.Items);
            if (item is not Chest) throw new InvalidIdException("NOT_CHEST", "Это не сундук.");
            return (Chest)item;
        }
        public BattleLog HitChest(int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);

            BattleLog battleLog;
            if (chest.IsMimic)
            {
                Session.CurrentRoom!.Items.Remove(chest);

                Enemy enemy = EnemyFactory.CreateMimic(Session.CurrentRoom!.Number, chest);
                Session.CurrentRoom!.Enemies.Add(enemy);
                Session.IsInBattle = true;
                battleLog = CombatRepository.DealDamage();
            }
            else
            {
                int playerHealthBeforeAttack = Session.CurrentHealth;
                int damage = Session.Weapon.Attack(Session);
                battleLog = new BattleLog("СУНДУК", damage, null, null, "ИГРОК", playerHealthBeforeAttack, Session.CurrentHealth);
            }
            return battleLog;
        }
        public void OpenChest(int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsMimic)
            {
                Session.IsGameStarted = false;
                throw new DefeatException("НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!", GameInfoRepository.GetGameInfo());
            }
            chest.Open();
        }
        public void UnlockChest(int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (Session.Keys > 0) Session.Keys--;
            else throw new NoKeyException();
            chest.Unlock();
        }
        public List<Item> SearchChest(int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsClosed) throw new ClosedException();
            return chest.Search();
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = GetItemByIdRepository.GetItemById(itemId, chest.Items);
            if (item is Coin) Session.Coins++;
            else if (item is Key) Session.Keys++;
            else Session.Inventory.Add(item);
            chest.Items.Remove(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
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
    public class GameInfoRepository : IGameInfoRepository
    {
        private GameSession Session;
        public GameInfoRepository(GameSession session)
        {
            Session = session;
        }
        public GameInfoDTO GetGameInfo()
        {
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            RoomDTO roomDTO = new RoomDTO(Session.CurrentRoom!.Number, Session.CurrentRoom!.Name!, Session.CurrentRoom!.Description!, Session.CurrentRoom!.Enemies);
            return new GameInfoDTO(roomDTO, Session.Weapon, Session.Helm, Session.Chestplate, Session.MaxHealth, Session.CurrentHealth, Session.Coins, Session.Keys, GameObjectMapper.ToDTO(Session.Inventory));
        }
    }
    public class GetRoomByIdRepository : IGetRoomByIdRepository
    {
        private GameSession Session;
        IGameInfoRepository GameInfoRepository;
        public GetRoomByIdRepository(
            GameSession session,
            IGameInfoRepository gameInfoRepository
            )
        {
            Session = session;
            GameInfoRepository = gameInfoRepository;
        }
        public Room GetRoomById(int roomId)
        {
            /*//Старый вариант
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");*/
            if (!Session.IsGameStarted && Session.Rooms.Count <= 1) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            if (roomId < 0 || roomId > Session.Rooms.Count) throw new NullRoomIdException();
            Room room = Session.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            Session.CurrentRoom = room;
            if (Session.CurrentRoom is EndRoom) throw new WinException(GameInfoRepository.GetGameInfo());
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
        private readonly IEnemyIdFactory EnemyIdFactory;

        private readonly IInventoryRepository InventoryRepository;
        private readonly IGameInfoRepository GameInfoRepository;
        public GameControllerRepository(
            GameSession session,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IRoomNumberFactory roomNumberFactory,
            IRoomFactory roomFactory,
            IItemIdFactory itemIdFactory,
            IInventoryRepository inventoryRepository,
            IEnemyIdFactory enemyIdFactory,
            IGameInfoRepository gameInfoRepository
            )
        {
            Session = session;
            GetCurrentRoomRepository = getCurrentRoomRepository;
            RoomNumberFactory = roomNumberFactory;
            RoomFactory = roomFactory;
            ItemIdFactory = itemIdFactory;
            InventoryRepository = inventoryRepository;
            EnemyIdFactory = enemyIdFactory;
            GameInfoRepository = gameInfoRepository;
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

            Session.IsInBattle = false;

            RoomNumberFactory.Reset();
            ItemIdFactory.Reset();
            EnemyIdFactory.Reset();
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
        public void UseInventoryItem(int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Heal heal) throw new InvalidIdException("NOT_HEAL", "Это не предмет лечения.");
            Session.Inventory.Remove(heal);
            heal.Use(Session);
            if (Session.CurrentHealth <= 0) throw new DefeatException($"{heal.Name} приводит Вас к гибели!", GetGameInfo());
        }

        public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        public List<Equipment> GetEquipment() => InventoryRepository.GetEquipment();
        public List<Equipment> EquipInventoryItem(int itemId) => InventoryRepository.EquipInventoryItem(itemId);
        public List<Equipment> UnequipWeapon() => InventoryRepository.UnequipWeapon();
        public List<Equipment> UnequipHelm() => InventoryRepository.UnequipHelm();
        public List<Equipment> UnequipChestplate() => InventoryRepository.UnequipChestplate();
        public GameInfoDTO GetGameInfo() => GameInfoRepository.GetGameInfo();
    }

    public class RoomControllerRepository : IRoomControllerRepository
    {
        private GameSession Session;
        private IGetCurrentRoomRepository GetCurrentRoomRepository;
        private IChestRepository ChestRepository;
        private IGameInfoRepository GameInfoRepository;
        private IGetRoomByIdRepository GetRoomByIdRepository;
        private IGetItemByIdRepository GetItemByIdRepository;
        private IGetEnemyByIdRepository GetEnemyByIdRepository;
        private ICombatRepository CombatRepository;
        public RoomControllerRepository(
            GameSession session,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IChestRepository chestRepository,
            IGameInfoRepository gameInfoRepository,
            IGetRoomByIdRepository getRoomByIdRepository,
            IGetItemByIdRepository getItemByIdRepository,
            IGetEnemyByIdRepository getEnemyByIdRepository,
            ICombatRepository combatRepository
            )
        {
            Session = session;
            GetCurrentRoomRepository = getCurrentRoomRepository;
            ChestRepository = chestRepository;
            GameInfoRepository = gameInfoRepository;
            GetRoomByIdRepository = getRoomByIdRepository;
            GetItemByIdRepository = getItemByIdRepository;
            CombatRepository = combatRepository;
            GetEnemyByIdRepository = getEnemyByIdRepository;
        }
        public Room GetCurrentRoom() => GetCurrentRoomRepository.GetCurrentRoom();
        public void GoNextRoom()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            Session.CurrentRoom = Session.Rooms[Session.CurrentRoom!.Number + 1];
            Session.CurrentRoom.IsDiscovered = true;
            if (Session.CurrentRoom is EndRoom) throw new WinException(GameInfoRepository.GetGameInfo());
            if (Session.CurrentRoom.Enemies.Any()) Session.IsInBattle = true;
        }
        public List<Item> Search()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            //Room room = GetRoomById(roomId);
            Room room = Session.CurrentRoom!;
            return room!.Items;
        }
        public void TakeItem(int itemId)
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            //Room room = GetRoomById(roomId);
            //Room room = Session.CurrentRoom!;
            Item item = GetItemByIdRepository.GetItemById(itemId, Session.CurrentRoom!.Items);
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is Coin) Session.Coins++;
            else if (item is Key) Session.Keys++;
            else Session.Inventory.Add(item);
            Session.CurrentRoom!.Items.Remove(item);
        }
        public void TakeAllItems()
        {
            if (!Session.IsGameStarted) throw new UnstartedGameException();
            if (Session.IsInBattle) throw new InBattleException();

            //Room room = GetRoomById(roomId);
            List<Item> carryableItems = Session.CurrentRoom!.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                if (!item.IsCarryable) continue;
                if (item is Coin) Session.Coins++;
                else if (item is Key) Session.Keys++;
                else Session.Inventory.Add(item);
            }
            Session.CurrentRoom!.Items.RemoveAll(x => x.IsCarryable);
        }
        //public List<Enemy> GetEnemies(int roomId) => GetEnemyByIdRepository.GetEnemies();
        public Enemy GetEnemyById() => GetEnemyByIdRepository.GetEnemyById();

        public BattleLog DealDamage() => CombatRepository.DealDamage();
        public BattleLog GetDamage() => CombatRepository.GetDamage();

        public ChestDTO ReturnChestDTO(Chest chest) => ChestRepository.ReturnChestDTO(chest);
        public ChestDTO ReturnChestDTO(int chestId) => ChestRepository.ReturnChestDTO(chestId);
        public BattleLog HitChest(int chestId) => ChestRepository.HitChest(chestId);
        public void OpenChest(int chestId) => ChestRepository.OpenChest(chestId);
        public void UnlockChest(int chestId) => ChestRepository.UnlockChest(chestId);
        public List<Item> SearchChest(int chestId) => ChestRepository.SearchChest(chestId);
        public void TakeItemFromChest(int chestId, int itemId) => ChestRepository.TakeItemFromChest(chestId, itemId);
        public void TakeAllItemsFromChest(int chestId) => ChestRepository.TakeAllItemsFromChest(chestId);
        public Room GetRoomById(int roomId) => GetRoomByIdRepository.GetRoomById(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => GameInfoRepository.GetGameInfo();
    }
}