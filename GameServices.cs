namespace TextGame
{
    public class CombatRepository : ICombatRepository
    {
        private readonly GameSession _session;
        private readonly IGetEnemyByIdRepository _getEnemyByIdRepository;
        private readonly IGameInfoRepository _gameInfoRepository;
        public CombatRepository(
            GameSession gameSession,
            IGetEnemyByIdRepository getEnemyByIdRepository,
            IGameInfoRepository gameInfoRepository
            )
        {
            _session = gameSession;
            _getEnemyByIdRepository = getEnemyByIdRepository;
            _gameInfoRepository = gameInfoRepository;
        }
        public BattleLog DealDamage()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            int playerHealthBeforeAttack = _session.CurrentHealth;
            Enemy enemy = _getEnemyByIdRepository.GetEnemyById();
            int damage = _session.Weapon.Attack(_session);
            int enemyHealthBeforeAttack = enemy.Health;
            int enemyHealthAfterAttack = enemy.GetDamage(damage, _session.CurrentRoom!);
            int playerHealthAfterAttack = playerHealthBeforeAttack - _session.CurrentHealth;
            BattleLog battleLog = new BattleLog(enemy.Name!, damage, enemyHealthBeforeAttack, enemyHealthAfterAttack, "ИГРОК", playerHealthAfterAttack, playerHealthBeforeAttack, _session.CurrentHealth);
            if (enemyHealthAfterAttack <= 0)
            {
                _session.CurrentRoom!.Enemies.Remove(enemy);
                if (_session.CurrentMimicChest is not null)
                {
                    _session.CurrentMimicChest.KillMimic();
                    _session.CurrentRoom.Items.Add(_session.CurrentMimicChest);
                    _session.CurrentMimicChest = null;
                }
                if (_session.CurrentHealth <= 0) throw new DefeatException("Вы погибли от своей же атаки. Как отчаянно.", _gameInfoRepository.GetGameInfo());//дубль
                if (!_session.CurrentRoom.Enemies.Any()) _session.IsInBattle = false;
                throw new BattleWinException($"{enemy.Name!} повержен.", battleLog);
            }
            if (_session.CurrentHealth <= 0) throw new DefeatException("Вы погибли от своей же атаки. Как отчаянно.", _gameInfoRepository.GetGameInfo()); //дубль
            return battleLog;
        }
        public BattleLog GetDamage()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            Enemy enemy = _getEnemyByIdRepository.GetEnemyById();
            int enemyHealthBeforeAttack = enemy.Health;
            int damage = enemy.Attack();
            int helmBlock = _session.Helm != null ? _session.Helm.Block(_session) : 0;
            int chestplateBlock = _session.Chestplate != null ? _session.Chestplate.Block(_session) : 0;
            int damageAfterBlock = damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = _session.CurrentHealth;
            if (damageAfterBlock > 0) _session.CurrentHealth -= damageAfterBlock;
            if (_session.CurrentHealth <= 0) throw new DefeatException($"Вы были повержены {enemy.Name}ОМ.", _gameInfoRepository.GetGameInfo());
            int enemyHealthAfterAttack = enemyHealthBeforeAttack - enemy.Health;
            return new BattleLog("ИГРОК", damage, playerHealthBeforeAttack, _session.CurrentHealth, enemy.Name!, enemyHealthAfterAttack, enemyHealthBeforeAttack, enemy.Health);
        }
    }
    public class GetEnemyByIdRepository : IGetEnemyByIdRepository
    {
        private readonly GameSession _session;
        public GetEnemyByIdRepository(GameSession session)
        {
            _session = session;
        }
        public Enemy GetEnemyById()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            Room room = _session.CurrentRoom!;
            Enemy? enemy = room.Enemies.FirstOrDefault();
            if (enemy == null) throw new NullEnemyIdException();
            return enemy;
        }
    }
    public class GetCurrentRoomRepository : IGetCurrentRoomRepository
    {
        private readonly GameSession _session;
        public GetCurrentRoomRepository(GameSession session)
        {
            _session = session;
        }
        public Room GetCurrentRoom()
        {
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            return _session.CurrentRoom!;
        }
    }
    public class InventoryRepository : IInventoryRepository
    {
        private readonly GameSession _session;
        public InventoryRepository(GameSession session)
        {
            _session = session;
        }

        public Item GetInventoryItem(int itemId)
        {
            Item? item = _session.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = new List<Equipment>() { _session.Weapon };
            if (_session.Helm != null) equipmentList.Add(_session.Helm);
            if (_session.Chestplate != null) equipmentList.Add(_session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> EquipInventoryItem(int itemId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Equipment equipment) throw new InvalidIdException("NOT_EQUIPMENT", "Это не снаряжение.");
            switch (equipment)
            {
                case Weapon weapon:
                    if (_session.Weapon != Fists.DefaultFists) _session.Inventory.Add(_session.Weapon);
                    _session.Weapon = weapon;
                    _session.Inventory.Remove(weapon);
                    break;
                case Armor armor:
                    switch (armor)
                    {
                        case Helm helm:
                            if (_session.Helm != null) _session.Inventory.Add(_session.Helm);
                            _session.Helm = helm;
                            _session.Inventory.Remove(helm);
                            break;
                        case Chestplate chestplate:
                            if (_session.Chestplate != null) _session.Inventory.Add(_session.Chestplate);
                            _session.Chestplate = chestplate;
                            _session.Inventory.Remove(chestplate);
                            break;
                    }
                    break;
            }
            List<Equipment> equipmentList = new List<Equipment>() { _session.Weapon };
            if (_session.Helm != null) equipmentList.Add(_session.Helm);
            if (_session.Chestplate != null) equipmentList.Add(_session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipWeapon()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            if (_session.Weapon == Fists.DefaultFists) throw new EmptyException();
            _session.Inventory.Add(_session.Weapon);
            _session.Weapon = Fists.DefaultFists;
            List<Equipment> equipmentList = new List<Equipment>() { _session.Weapon };
            if (_session.Helm != null) equipmentList.Add(_session.Helm);
            if (_session.Chestplate != null) equipmentList.Add(_session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipHelm()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            if (_session.Helm == null) throw new EmptyException();
            _session.Inventory.Add(_session.Helm!);
            _session.Helm = null;
            List<Equipment> equipmentList = new List<Equipment>() { _session.Weapon };
            if (_session.Helm != null) equipmentList.Add(_session.Helm);
            if (_session.Chestplate != null) equipmentList.Add(_session.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipChestplate()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            if (_session.Chestplate == null) throw new EmptyException();
            _session.Inventory.Add(_session.Chestplate!);
            _session.Chestplate = null;
            List<Equipment> equipmentList = new List<Equipment>() { _session.Weapon };
            if (_session.Helm != null) equipmentList.Add(_session.Helm);
            if (_session.Chestplate != null) equipmentList.Add(_session.Chestplate);
            return equipmentList;
        }
        public void SellInventoryItem(int itemId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            if (_session.CurrentRoom is not Shop) throw new NotShopException();
            Item item = GetInventoryItem(itemId);
            if (item.Cost == null) throw new UnsellableItemException();

            _session.Inventory.Remove(item);
            _session.Coins += (int)item.Cost;
        }
    }
    public class ChestRepository : IChestRepository
    {
        private readonly GameSession _session;

        private IGameInfoRepository _gameInfoRepository;
        private IGetItemByIdRepository _getItemByIdRepository;
        private ICombatRepository _combatRepository;
        public ChestRepository(
            GameSession session,
            IGameInfoRepository gameInfoRepository,
            IGetItemByIdRepository getItemByIdRepository,
            ICombatRepository combatRepository
            )
        {
            _session = session;
            _gameInfoRepository = gameInfoRepository;
            _getItemByIdRepository = getItemByIdRepository;
            _combatRepository = combatRepository;
        }
        public ChestStateDTO ReturnChestDTO(Chest chest)
        {
            return new ChestStateDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public ChestStateDTO ReturnChestDTO(int chestId)
        {
            Chest chest = GetChestById(chestId);
            return new ChestStateDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public Chest GetChestById(int chestId)
        {
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);
            Item item = _getItemByIdRepository.GetItemById(chestId, _session.CurrentRoom!.Items);
            if (item is not Chest) throw new InvalidIdException("NOT_CHEST", "Это не сундук.");
            return (Chest)item;
        }
        public BattleLog HitChest(int chestId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);

            BattleLog battleLog;
            if (chest.Mimic is not null)
            {
                _session.CurrentMimicChest = chest;
                _session.CurrentRoom!.Items.Remove(chest);
                _session.CurrentRoom!.Enemies.Add(chest.Mimic);
                _session.IsInBattle = true;
                battleLog = _combatRepository.DealDamage();
            }
            else
            {
                int playerHealthBeforeAttack = _session.CurrentHealth;
                int damage = _session.Weapon.Attack(_session);
                int playerHealthAfterAttack = playerHealthBeforeAttack - _session.CurrentHealth;
                battleLog = new BattleLog("СУНДУК", damage, null, null, "ИГРОК", playerHealthAfterAttack, playerHealthBeforeAttack, _session.CurrentHealth);
            }
            return battleLog;
        }
        public void OpenChest(int chestId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.Mimic is not null)
            {
                _session.IsGameStarted = false;
                throw new DefeatException("НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!", _gameInfoRepository.GetGameInfo());
            }
            chest.Open();
        }
        public void UnlockChest(int chestId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (_session.Keys > 0) _session.Keys--;
            else throw new NoKeyException();
            chest.Unlock();
        }
        public List<Item> SearchChest(int chestId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsClosed) throw new ClosedException();
            return chest.Search();
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = _getItemByIdRepository.GetItemById(itemId, chest.Items);
            if (item is BagOfCoins bagOfCoins) _session.Coins += (int)bagOfCoins.Cost!;
            else if (item is Key) _session.Keys++;
            else _session.Inventory.Add(item);
            chest.Items.Remove(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            List<Item> carryableItems = chest.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                if (item is BagOfCoins bagOfCoins) _session.Coins += (int)bagOfCoins.Cost!;
                else if (item is Key) _session.Keys++;
                else _session.Inventory.Add(item);
            }
            chest.Items.RemoveAll(x => x.IsCarryable);
        }
    }
    public class GameInfoRepository : IGameInfoRepository
    {
        private readonly GameSession _session;
        public GameInfoRepository(GameSession session)
        {
            _session = session;
        }
        public GameInfoDTO GetGameInfo()
        {
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            //RoomDTO roomDTO = new RoomDTO(Session.CurrentRoom!.Number, Session.CurrentRoom!.Name!, Session.CurrentRoom!.Description!, Session.CurrentRoom!.Enemies);
            var roomDTO = GameObjectMapper.ToDTO(_session.CurrentRoom!);
            WeaponDTO weaponDTO = (WeaponDTO)GameObjectMapper.ToDTO(_session.Weapon);
            ArmorDTO? helmDTO = _session.Helm != null ? (ArmorDTO)GameObjectMapper.ToDTO(_session.Helm) : null;
            ArmorDTO? chestplateDTO = _session.Chestplate != null ? (ArmorDTO)GameObjectMapper.ToDTO(_session.Chestplate) : null;
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _session.MaxHealth, _session.CurrentHealth, _session.Coins, _session.Keys, GameObjectMapper.ToDTO(_session.Inventory));
        }
    }
    public class GetRoomByIdRepository : IGetRoomByIdRepository
    {
        private readonly GameSession _session;
        private readonly IGameInfoRepository _gameInfoRepository;
        public GetRoomByIdRepository(
            GameSession session,
            IGameInfoRepository gameInfoRepository
            )
        {
            _session = session;
            _gameInfoRepository = gameInfoRepository;
        }
        public Room GetRoomById(int roomId)
        {
            /*//Старый вариант
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");*/
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            if (roomId < 0 || roomId > _session.Rooms.Count) throw new NullRoomIdException();
            Room room = _session.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            _session.CurrentRoom = room;
            if (_session.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
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
        private readonly GameSession _session;
        private readonly IGetCurrentRoomRepository _getCurrentRoomRepository;

        private readonly IRoomNumberFactory _roomNumberFactory;
        private readonly IRoomFactory _roomFactory;
        private readonly IItemIdFactory _itemIdFactory;
        private readonly IEnemyIdFactory _enemyIdFactory;

        private readonly IInventoryRepository _inventoryRepository;
        private readonly IGameInfoRepository _gameInfoRepository;
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
            _session = session;
            _getCurrentRoomRepository = getCurrentRoomRepository;
            _roomNumberFactory = roomNumberFactory;
            _roomFactory = roomFactory;
            _itemIdFactory = itemIdFactory;
            _inventoryRepository = inventoryRepository;
            _enemyIdFactory = enemyIdFactory;
            _gameInfoRepository = gameInfoRepository;
        }
        public Room GetCurrentRoom() => _getCurrentRoomRepository.GetCurrentRoom();
        public void Start()
        {
            ResetGame();
            _session.Rooms = GenerateMap();
            _session.CurrentRoom = _session.Rooms[0];
            _session.IsGameStarted = true;
        }
        public void ResetGame()
        {
            _session.Coins = 0;
            _session.Keys = 0;

            _session.Weapon = Fists.DefaultFists;
            _session.Helm = null;
            _session.Chestplate = null;

            _session.Inventory = new List<Item>();
            _session.Rooms = new List<Room>();

            _session.MaxHealth = GameSession.DefaultMaxHealth;
            _session.CurrentHealth = GameSession.DefaultMaxHealth;

            _session.IsInBattle = false;

            _roomNumberFactory.Reset();
            _itemIdFactory.Reset();
            _enemyIdFactory.Reset();
        }
        public List<Room> GenerateMap()
        {
            List<Room> rooms = new List<Room>()
            {
                new StartRoom(),
            };
            while (rooms.Last() is not EndRoom)
            {
                rooms.Add(_roomFactory.CreateRoom());
            }
            return rooms;
        }
        public List<Item> GetInventory()
        {
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            return _session.Inventory;
        }
        public int GetCoins()
        {
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            return _session.Coins;
        }
        public int GetKeys()
        {
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            return _session.Keys;
        }
        public List<MapRoomDTO> GetMap()
        {
            if (!_session.IsGameStarted && _session.Rooms.Count <= 1) throw new UnstartedGameException();
            if (!_session.Inventory.OfType<Map>().Any()) throw new NoMapException();
            return _session.Rooms.Select(r => new MapRoomDTO(r.Number, r.Name ?? "НЕИЗВЕСТНО")).ToList();
        }
        public void UseInventoryItem(int itemId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Heal heal) throw new InvalidIdException("NOT_HEAL", "Это не предмет лечения.");
            _session.Inventory.Remove(heal);
            heal.Use(_session);
            if (_session.CurrentHealth <= 0) throw new DefeatException($"{heal.Name} приводит Вас к гибели!", GetGameInfo());
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

    public class RoomControllerRepository : IRoomControllerRepository
    {
        private readonly GameSession _session;
        private readonly IGetCurrentRoomRepository _getCurrentRoomRepository;
        private readonly IChestRepository _chestRepository;
        private readonly IGameInfoRepository _gameInfoRepository;
        private readonly IGetRoomByIdRepository _getRoomByIdRepository;
        private readonly IGetItemByIdRepository _getItemByIdRepository;
        private readonly IGetEnemyByIdRepository _getEnemyByIdRepository;
        private readonly ICombatRepository _combatRepository;
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
            _session = session;
            _getCurrentRoomRepository = getCurrentRoomRepository;
            _chestRepository = chestRepository;
            _gameInfoRepository = gameInfoRepository;
            _getRoomByIdRepository = getRoomByIdRepository;
            _getItemByIdRepository = getItemByIdRepository;
            _combatRepository = combatRepository;
            _getEnemyByIdRepository = getEnemyByIdRepository;
        }
        public Room GetCurrentRoom() => _getCurrentRoomRepository.GetCurrentRoom();
        public void GoNextRoom()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            _session.CurrentRoom = _session.Rooms[_session.CurrentRoom!.Number + 1];
            _session.CurrentRoom.IsDiscovered = true;
            if (_session.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
            if (_session.CurrentRoom.Enemies.Any()) _session.IsInBattle = true;
        }
        public List<Item> Search()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();

            //Room room = GetRoomById(roomId);
            Room room = _session.CurrentRoom!;
            room.IsSearched = true;
            return room!.Items;
        }
        public void TakeItem(int itemId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();
            if (!_session.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
            if (_session.CurrentRoom is Shop) throw new ImpossibleStealException();

            //Room room = GetRoomById(roomId);
            //Room room = Session.CurrentRoom!;
            Item item = _getItemByIdRepository.GetItemById(itemId, _session.CurrentRoom!.Items);
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is BagOfCoins bagOfCoins) _session.Coins += (int)bagOfCoins.Cost!;
            else if (item is Key) _session.Keys++;
            else _session.Inventory.Add(item);
            _session.CurrentRoom!.Items.Remove(item);
        }
        public void TakeAllItems()
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();
            if (!_session.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
            if (_session.CurrentRoom is Shop) throw new ImpossibleStealException();

            //Room room = GetRoomById(roomId);
            List<Item> carryableItems = _session.CurrentRoom!.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                if (!item.IsCarryable) continue;
                if (item is BagOfCoins bagOfCoins) _session.Coins += (int)bagOfCoins.Cost!;
                else if (item is Key) _session.Keys++;
                else _session.Inventory.Add(item);
            }
            _session.CurrentRoom!.Items.RemoveAll(x => x.IsCarryable);
        }
        public void BuyItem(int itemId)
        {
            if (!_session.IsGameStarted) throw new UnstartedGameException();
            if (_session.IsInBattle) throw new InBattleException();
            if (!_session.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();

            if (_session.CurrentRoom is not Shop) throw new NotShopException();
            Item item = _getItemByIdRepository.GetItemById(itemId, _session.CurrentRoom.Items);
            if (item.Cost > _session.Coins) throw new NoMoneyException();

            _session.Coins -= (int)item.Cost!;
            _session.CurrentRoom.Items.Remove(item);
            _session.Inventory.Add(item);
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
        public List<Item> SearchChest(int chestId) => _chestRepository.SearchChest(chestId);
        public void TakeItemFromChest(int chestId, int itemId) => _chestRepository.TakeItemFromChest(chestId, itemId);
        public void TakeAllItemsFromChest(int chestId) => _chestRepository.TakeAllItemsFromChest(chestId);
        public Room GetRoomById(int roomId) => _getRoomByIdRepository.GetRoomById(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => _gameInfoRepository.GetGameInfo();
    }
}