using System;

namespace TextGame
{
    public class GameSessionService : IGameSessionService
    {
        private GameSession _session;
        private readonly IMapGenerator _mapGenerator;
        public GameSessionService(GameSession session, IMapGenerator mapGenerator)
        {
            _session = session;
            _mapGenerator = mapGenerator;
        }
        public bool IsGameStarted => _session.IsGameStarted;
        public bool IsInBattle { get => _session.IsInBattle; }
        public Room CurrentRoom { get => _session.CurrentRoom!; }
        public IReadOnlyList<Room> Rooms => _session.Rooms.AsReadOnly();
        public IReadOnlyList<Item> Inventory => _session.Inventory.AsReadOnly();
        public int Coins { get => _session.Coins; }
        public int Keys { get => _session.Keys; }
        public Weapon Weapon { get => _session.Weapon; }
        public Helm? Helm { get => _session.Helm; }
        public Chestplate? Chestplate { get => _session.Chestplate; }
        public int MaxHealth { get => _session.MaxHealth; }
        public int CurrentHealth { get => _session.CurrentHealth; }
        public Chest? CurrentMimicChest { get => _session.CurrentMimicChest; }
        public int RoomCounter { get => _session.RoomCounter; }
        public int ItemCounter { get => _session.ItemCounter; }
        public int EnemyCounter { get => _session.EnemyCounter; }


        public int NextRoomNumber() => ++_session.RoomCounter;
        public int NextItemId() => ++_session.ItemCounter;
        public int NextEnemyId() => ++_session.EnemyCounter;


        public void RemoveWeapon() => _session.Weapon = Fists.DefaultFists;
        public void EquipWeapon(Weapon weapon) => _session.Weapon = weapon;
        public void RemoveChestplate() => _session.Chestplate = null;
        public void EquipChestplate(Chestplate chestplate) => _session.Chestplate = chestplate;
        public void RemoveHelm() => _session.Helm = null;
        public void EquipHelm(Helm helm) => _session.Helm = helm;

        public void AddMaxHealth(int value)
        {
            if (value == 0) return;

            _session.MaxHealth += value;
            _session.CurrentHealth += value;
        }
        public void AddCurrentHealth(int value)
        {
            if (value == 0) return;

            if ((CurrentHealth + value) >= MaxHealth) _session.CurrentHealth = MaxHealth;
            else _session.CurrentHealth += value;
        }
        public void RemoveCurrentMimicChest() => _session.CurrentMimicChest = null;
        public void SetCurrentMimicChest(Chest chest) => _session.CurrentMimicChest = chest;

        public void StartGame()
        {
            _session.Coins = 0;
            _session.Keys = 0;

            RemoveWeapon();
            RemoveHelm();
            RemoveChestplate();

            _session.Inventory = new List<Item>();
            _session.Rooms = new List<Room>();

            _session.MaxHealth = GameBalance.DefaultMaxHealth;
            _session.CurrentHealth = GameBalance.DefaultMaxHealth;


            _session.RoomCounter = 0;
            _session.ItemCounter = 0;
            _session.EnemyCounter = 0;

            _session.Rooms = _mapGenerator.Generate(this);
            SetCurrentRoom(Rooms[0]);

            EndBattle();
            _session.IsGameStarted = true;
        }
        public void EndGame() => _session.IsGameStarted = false;
        public void StartBattle()
        {
            if (CurrentRoom.Enemies.Any()) _session.IsInBattle = true;
        }
        public void EndBattle()
        {
            if (!CurrentRoom.Enemies.Any()) _session.IsInBattle = false;
        }
        public void AddCoins(int value) => _session.Coins += value;
        public void AddKeys(int value) => _session.Keys += value;
        public void AddItemToInventory(Item item) => _session.Inventory.Add(item);
        public void RemoveItemFromInventory(Item item) => _session.Inventory.Remove(item);

        public void SetCurrentRoom(Room room) => _session.CurrentRoom = room;
        public void AddEnemyToCurrentRoom(Enemy enemy) => _session.CurrentRoom!.AddEnemy(enemy);
        public void RemoveEnemyFromCurrentRoom(Enemy enemy) => _session.CurrentRoom!.RemoveEnemy(enemy);
        public void AddItemToCurrentRoom(Item item) => _session.CurrentRoom!.AddItem(item);
        public void RemoveItemFromCurrentRoom(Item item) => _session.CurrentRoom!.RemoveItem(item);
    }
    public class MapGenerator : IMapGenerator
    {
        private readonly IRoomFactory _roomFactory;
        public MapGenerator(IRoomFactory roomFactory) 
        {
            _roomFactory = roomFactory;
        }
        public List<Room> Generate(IGameSessionService sessionService)
        {
            var rooms = new List<Room> { _roomFactory.CreateStartRoom() };
            var options = new (int Weight, Func<Room> Create)[]
            {
            (GameBalance.EmptyRoomWeight, () => _roomFactory.CreateEmptyRoom(sessionService)),
            (GameBalance.SmallRoomWeight, () => _roomFactory.CreateSmallRoom(sessionService)),
            (GameBalance.BigRoomWeight, () => _roomFactory.CreateBigRoom(sessionService)),
            (GameBalance.ShopWeight, () => _roomFactory.CreateShopRoom(sessionService))
            };
            int baseWeightSum = options.Sum(x => x.Weight);

            while (rooms.Last() is not EndRoom)
            {
                int endRoomWeight = sessionService.RoomCounter;
                int totalWeight = baseWeightSum + endRoomWeight;

                int roll = Random.Shared.Next(totalWeight);

                if (roll >= baseWeightSum)
                {
                    rooms.Add(_roomFactory.CreateEndRoom(sessionService));
                }
                else
                {
                    int acc = 0;
                    foreach (var (weight, create) in options)
                    {
                        if (roll < acc + weight)
                        {
                            rooms.Add(create());
                            break;
                        }
                        acc += weight;
                    }
                }
            }

            return rooms;
        }
    }
    public class CombatRepository : ICombatRepository
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGetEnemyByIdRepository _getEnemyByIdRepository;
        private readonly IGameInfoRepository _gameInfoRepository;
        public CombatRepository(
            IGameSessionService sessionService,
            IGetEnemyByIdRepository getEnemyByIdRepository,
            IGameInfoRepository gameInfoRepository
            )
        {
            _sessionService = sessionService;
            _getEnemyByIdRepository = getEnemyByIdRepository;
            _gameInfoRepository = gameInfoRepository;
        }
        public BattleLog DealDamage()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            int playerHealthBeforeAttack = _sessionService.CurrentHealth;
            Enemy enemy = _getEnemyByIdRepository.GetEnemyById();
            int damage = _sessionService.Weapon.Attack(_sessionService);
            int enemyHealthBeforeAttack = enemy.Health;
            int enemyHealthAfterAttack = enemy.GetDamage(damage, _sessionService.CurrentRoom!);
            int playerHealthAfterAttack = playerHealthBeforeAttack - _sessionService.CurrentHealth;
            BattleLog battleLog = new BattleLog(enemy.Name!, damage, enemyHealthBeforeAttack, enemyHealthAfterAttack, "ИГРОК", playerHealthAfterAttack, playerHealthBeforeAttack, _sessionService.CurrentHealth);
            if (enemyHealthAfterAttack <= 0)
            {
                _sessionService.RemoveEnemyFromCurrentRoom(enemy);
                if (_sessionService.CurrentMimicChest is not null)
                {
                    _sessionService.CurrentMimicChest.KillMimic();
                    _sessionService.AddItemToCurrentRoom(_sessionService.CurrentMimicChest);
                    _sessionService.RemoveCurrentMimicChest();
                }
                CheckPlayerHealthAfterAttack();
                _sessionService.EndBattle();
                throw new BattleWinException($"{enemy.Name!} повержен.", battleLog);
            }
            CheckPlayerHealthAfterAttack();
            return battleLog;
        }
        public void CheckPlayerHealthAfterAttack()
        {
            if (_sessionService.CurrentHealth <= 0) throw new DefeatException("Вы погибли от своей же атаки. Как отчаянно.", _gameInfoRepository.GetGameInfo());
        }
        public BattleLog GetDamage()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            Enemy enemy = _getEnemyByIdRepository.GetEnemyById();
            int enemyHealthBeforeAttack = enemy.Health;
            int damage = enemy.Attack();
            int helmBlock = _sessionService.Helm != null ? _sessionService.Helm.Block(_sessionService) : 0;
            int chestplateBlock = _sessionService.Chestplate != null ? _sessionService.Chestplate.Block(_sessionService) : 0;
            int damageAfterBlock = damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = _sessionService.CurrentHealth;
            if (damageAfterBlock > 0) _sessionService.AddCurrentHealth(-damageAfterBlock);
            if (_sessionService.CurrentHealth <= 0) throw new DefeatException($"Вы были повержены {enemy.Name}ОМ.", _gameInfoRepository.GetGameInfo());
            int enemyHealthAfterAttack = enemyHealthBeforeAttack - enemy.Health;
            return new BattleLog("ИГРОК", damage, playerHealthBeforeAttack, _sessionService.CurrentHealth, enemy.Name!, enemyHealthAfterAttack, enemyHealthBeforeAttack, enemy.Health);
        }
    }
    public class GetEnemyByIdRepository : IGetEnemyByIdRepository
    {
        private readonly IGameSessionService _sessionService;
        public GetEnemyByIdRepository(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public Enemy GetEnemyById()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            Room room = _sessionService.CurrentRoom!;
            Enemy? enemy = room.Enemies.FirstOrDefault();
            if (enemy == null) throw new NullEnemyIdException();
            return enemy;
        }
    }
    public class GetCurrentRoomRepository : IGetCurrentRoomRepository
    {
        private readonly IGameSessionService _sessionService;
        public GetCurrentRoomRepository(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public Room GetCurrentRoom()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            return _sessionService.CurrentRoom!;
        }
    }
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IGameSessionService _sessionService;
        public InventoryRepository(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public Item GetInventoryItem(int itemId)
        {
            Item? item = _sessionService.Inventory.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
        public List<Equipment> GetEquipment()
        {
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> EquipInventoryItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            Item item = GetInventoryItem(itemId);
            if (item is not Equipment equipment) throw new InvalidIdException("NOT_EQUIPMENT", "Это не снаряжение.");
            switch (equipment)
            {
                case Weapon weapon:
                    if (_sessionService.Weapon != Fists.DefaultFists) _sessionService.AddItemToInventory(_sessionService.Weapon);
                    _sessionService.EquipWeapon(weapon);
                    _sessionService.RemoveItemFromInventory(weapon);
                    break;
                case Armor armor:
                    switch (armor)
                    {
                        case Helm helm:
                            if (_sessionService.Helm != null) _sessionService.AddItemToInventory(_sessionService.Helm);
                            _sessionService.EquipHelm(helm);
                            _sessionService.RemoveItemFromInventory(helm);
                            break;
                        case Chestplate chestplate:
                            if (_sessionService.Chestplate != null) _sessionService.AddItemToInventory(_sessionService.Chestplate);
                            _sessionService.EquipChestplate(chestplate);
                            _sessionService.RemoveItemFromInventory(chestplate);
                            break;
                    }
                    break;
            }
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipWeapon()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            if (_sessionService.Weapon == Fists.DefaultFists) throw new EmptyException();
            _sessionService.AddItemToInventory(_sessionService.Weapon);
            _sessionService.RemoveWeapon();
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipHelm()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            if (_sessionService.Helm == null) throw new EmptyException();
            _sessionService.AddItemToInventory(_sessionService.Helm!);
            _sessionService.RemoveHelm();
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public List<Equipment> UnequipChestplate()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();

            if (_sessionService.Chestplate == null) throw new EmptyException();
            _sessionService.AddItemToInventory(_sessionService.Chestplate!);
            _sessionService.RemoveChestplate();
            List<Equipment> equipmentList = new List<Equipment>() { _sessionService.Weapon };
            if (_sessionService.Helm != null) equipmentList.Add(_sessionService.Helm);
            if (_sessionService.Chestplate != null) equipmentList.Add(_sessionService.Chestplate);
            return equipmentList;
        }
        public void SellInventoryItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            if (_sessionService.CurrentRoom is not Shop) throw new NotShopException();
            Item item = GetInventoryItem(itemId);
            if (item.Cost == null) throw new UnsellableItemException();

            _sessionService.RemoveItemFromInventory(item);
            _sessionService.AddCoins((int)item.Cost);
        }
    }
    public class CheckItemService : ICheckItemService
    {
        private IGameSessionService _sessionService;
        public CheckItemService(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public void CheckItem(Item item)
        {
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is BagOfCoins bagOfCoins) _sessionService.AddCoins((int)bagOfCoins.Cost!);
            else if (item is Key) _sessionService.AddKeys(1);
            else _sessionService.AddItemToInventory(item);
        }
    }
    public class ChestRepository : IChestRepository
    {
        private readonly IGameSessionService _sessionService;

        private IGameInfoRepository _gameInfoRepository;
        private IGetItemByIdRepository _getItemByIdRepository;
        private ICombatRepository _combatRepository;

        private ICheckItemService _checkItemService;
        public ChestRepository(
            IGameSessionService sessionService,
            IGameInfoRepository gameInfoRepository,
            IGetItemByIdRepository getItemByIdRepository,
            ICombatRepository combatRepository,
            ICheckItemService checkItemService
            )
        {
            _sessionService = sessionService;
            _gameInfoRepository = gameInfoRepository;
            _getItemByIdRepository = getItemByIdRepository;
            _combatRepository = combatRepository;
            _checkItemService = checkItemService;
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
            Item item = _getItemByIdRepository.GetItemById(chestId, _sessionService.CurrentRoom!.Items);
            if (item is not Chest) throw new InvalidIdException("NOT_CHEST", "Это не сундук.");
            return (Chest)item;
        }
        public BattleLog HitChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            //Room room = GetRoomByIdRepository.GetRoomById(roomId);

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
                int damage = _sessionService.Weapon.Attack(_sessionService);
                int playerHealthAfterAttack = playerHealthBeforeAttack - _sessionService.CurrentHealth;
                battleLog = new BattleLog("СУНДУК", damage, null, null, "ИГРОК", playerHealthAfterAttack, playerHealthBeforeAttack, _sessionService.CurrentHealth);
            }
            return battleLog;
        }
        public void OpenChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.Mimic is not null)
            {
                _sessionService.EndGame();
                throw new DefeatException("НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!", _gameInfoRepository.GetGameInfo());
            }
            chest.Open();
        }
        public void UnlockChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (_sessionService.Keys > 0) _sessionService.AddKeys(-1);
            else throw new NoKeyException();
            chest.Unlock();
        }
        public IEnumerable<Item> SearchChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsClosed) throw new ClosedException();
            return chest.Search();
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = _getItemByIdRepository.GetItemById(itemId, chest.Items);
            _checkItemService.CheckItem(item);
            chest.RemoveItem(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            List<Item> carryableItems = chest.Items.Where(i => i.IsCarryable == true).ToList();
            if (carryableItems.Count <= 0) throw new EmptyException();
            foreach (Item item in carryableItems)
            {
                _checkItemService.CheckItem(item);
                chest.RemoveItem(item);
            }
            //chest.Items.RemoveAll(x => x.IsCarryable);
        }
    }
    public class GameInfoRepository : IGameInfoRepository
    {
        private readonly IGameSessionService _sessionService;
        public GameInfoRepository(IGameSessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public GameInfoDTO GetGameInfo()
        {
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            //RoomDTO roomDTO = new RoomDTO(Session.CurrentRoom!.Number, Session.CurrentRoom!.Name!, Session.CurrentRoom!.Description!, Session.CurrentRoom!.Enemies);
            var roomDTO = GameObjectMapper.ToDTO(_sessionService.CurrentRoom!);
            WeaponDTO weaponDTO = (WeaponDTO)GameObjectMapper.ToDTO(_sessionService.Weapon);
            ArmorDTO? helmDTO = _sessionService.Helm != null ? (ArmorDTO)GameObjectMapper.ToDTO(_sessionService.Helm) : null;
            ArmorDTO? chestplateDTO = _sessionService.Chestplate != null ? (ArmorDTO)GameObjectMapper.ToDTO(_sessionService.Chestplate) : null;
            return new GameInfoDTO(roomDTO, weaponDTO, helmDTO, chestplateDTO, _sessionService.MaxHealth, _sessionService.CurrentHealth, _sessionService.Coins, _sessionService.Keys, GameObjectMapper.ToDTO(_sessionService.Inventory));
        }
    }
    public class GetRoomByIdRepository : IGetRoomByIdRepository
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGameInfoRepository _gameInfoRepository;
        public GetRoomByIdRepository(
            IGameSessionService sessionService,
            IGameInfoRepository gameInfoRepository
            )
        {
            _sessionService = sessionService;
            _gameInfoRepository = gameInfoRepository;
        }
        public Room GetRoomById(int roomId)
        {
            /*//Старый вариант
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");*/
            if (!_sessionService.IsGameStarted && _sessionService.Rooms.Count <= 1) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            if (roomId < 0 || roomId > _sessionService.Rooms.Count) throw new NullRoomIdException();
            Room room = _sessionService.Rooms[roomId];
            if (!room.IsDiscovered) throw new UndiscoveredRoomException();
            _sessionService.SetCurrentRoom(room);
            if (_sessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
            return room;
        }
    }
    public class GetItemByIdRepository : IGetItemByIdRepository
    {
        public Item GetItemById(int itemId, IEnumerable<Item> items)
        {
            Item? item = items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullItemIdException();
            return item;
        }
    }
    public class GameControllerRepository : IGameControllerRepository
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGetCurrentRoomRepository _getCurrentRoomRepository;

        private readonly IRoomFactory _roomFactory;

        private readonly IInventoryRepository _inventoryRepository;
        private readonly IGameInfoRepository _gameInfoRepository;
        public GameControllerRepository(
            IGameSessionService sessionService,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IRoomFactory roomFactory,
            IInventoryRepository inventoryRepository,
            IGameInfoRepository gameInfoRepository
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
            heal.Use(_sessionService);
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

    public class RoomControllerRepository : IRoomControllerRepository
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGetCurrentRoomRepository _getCurrentRoomRepository;
        private readonly IChestRepository _chestRepository;
        private readonly IGameInfoRepository _gameInfoRepository;
        private readonly IGetRoomByIdRepository _getRoomByIdRepository;
        private readonly IGetItemByIdRepository _getItemByIdRepository;
        private readonly IGetEnemyByIdRepository _getEnemyByIdRepository;
        private readonly ICombatRepository _combatRepository;
        private readonly ICheckItemService _checkItemService;
        public RoomControllerRepository(
            IGameSessionService sessionService,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IChestRepository chestRepository,
            IGameInfoRepository gameInfoRepository,
            IGetRoomByIdRepository getRoomByIdRepository,
            IGetItemByIdRepository getItemByIdRepository,
            IGetEnemyByIdRepository getEnemyByIdRepository,
            ICombatRepository combatRepository,
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
            _sessionService.CurrentRoom.IsDiscovered = true;
            if (_sessionService.CurrentRoom is EndRoom) throw new WinException(_gameInfoRepository.GetGameInfo());
            _sessionService.StartBattle();
        }
        public IEnumerable<Item> Search()
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();

            //Room room = GetRoomById(roomId);
            Room room = _sessionService.CurrentRoom!;
            room.IsSearched = true;
            return room!.Items;
        }
        public void TakeItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();
            if (!_sessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();
            if (_sessionService.CurrentRoom is Shop) throw new ImpossibleStealException();

            //Room room = GetRoomById(roomId);
            //Room room = Session.CurrentRoom!;
            Item item = _getItemByIdRepository.GetItemById(itemId, _sessionService.CurrentRoom!.Items);
            _checkItemService.CheckItem(item);
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
                _checkItemService.CheckItem(item);
                _sessionService.RemoveItemFromCurrentRoom(item);
            }
        }
        public void BuyItem(int itemId)
        {
            if (!_sessionService.IsGameStarted) throw new UnstartedGameException();
            if (_sessionService.IsInBattle) throw new InBattleException();
            if (!_sessionService.CurrentRoom!.IsSearched) throw new UnsearchedRoomException();

            if (_sessionService.CurrentRoom is not Shop) throw new NotShopException();
            Item item = _getItemByIdRepository.GetItemById(itemId, _sessionService.CurrentRoom.Items);
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
        public Room GetRoomById(int roomId) => _getRoomByIdRepository.GetRoomById(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => _gameInfoRepository.GetGameInfo();
    }
}