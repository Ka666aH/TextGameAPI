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

        public List<Item> SearchCurrentRoom() => CurrentRoom!.Search();
    }
    public class RoomFactory : IRoomFactory
    {
        public StartRoom CreateStartRoom() => new StartRoom("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую Вы сюда провалились.", 0);
        public EndRoom CreateEndRoom(int roomNumber) => new EndRoom("ВЫХОД", "Выход наружу. Свобода.", roomNumber);
        public EmptyRoom CreateEmptyRoom(int roomNumber) => new EmptyRoom("ПУСТАЯ КОМНАТА", "Ничего интересного.", roomNumber);
        public SmallRoom CreateSmallRoom(int roomNumber) => new SmallRoom("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", roomNumber);
        public BigRoom CreateBigRoom(int roomNumber) => new BigRoom("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", roomNumber);
        public Shop CreateShopRoom(int roomNumber) => new Shop("МАГАЗИН", "Здесь мутный торгаш продаёт своё добро.", roomNumber);
    }
    public interface IItemFactory
    {
        public Item? CreateRoomItem(IGameSessionService sessionService);
        public Item? CreateChestItem(IGameSessionService sessionService);
        public Item? CreateShopItem(IGameSessionService sessionService);
    }
    public class ItemFactory : IItemFactory
    {
        private readonly IEnemyFactory _enemyFactory;
        public ItemFactory(IEnemyFactory enemyFactory)
        {

            _enemyFactory = enemyFactory;
        }

        private Item? SelectRandom(List<(int Weight, Func<Item?> Creator)> options)
        {
            // Суммируем все веса
            int totalWeight = options.Sum(option => option.Weight);

            // Защита от пустого списка или нулевых весов
            if (totalWeight <= 0)
                return null;

            // Генерируем случайное число в диапазоне [0, totalWeight)
            int roll = Random.Shared.Next(totalWeight);
            int accumulated = 0;

            // Ищем предмет, в чей диапазон попало число
            foreach (var (weight, creator) in options)
            {
                if (roll < accumulated + weight)
                    return creator(); // Создаём предмет только когда он выбран

                accumulated += weight;
            }

            // На случай ошибок округления (очень редко)
            return options[^1].Creator();
        }
        private void AddWeightedGroup(
    List<(int Weight, Func<Item?> Creator)> options,
    int roomId,
    double groupWeightWeight,
    params (Func<int, double> WeightCalc, Func<Item?> Creator)[] items)
        {
            // 1. Считаем "сырые" веса для предметов в группе (без отрицательных значений)
            var rawWeights = items.Select(item => Math.Max(0, item.WeightCalc(roomId))).ToArray();

            // 2. Сумма всех весов в группе
            double groupSum = rawWeights.Sum();

            // 3. Если группа пустая - пропускаем
            if (groupSum <= 0)
                return;

            // 4. Добавляем каждый предмет с нормализованным весом
            for (int i = 0; i < items.Length; i++)
            {
                double rawWeight = rawWeights[i];
                if (rawWeight <= 0)
                    continue; // Пропускаем предметы с нулевым весом

                // Абсолютный вес = (вес_группы_в_процентах) * (вес_предмета / сумма_весов_группы)
                int absoluteWeight = (int)(groupWeightWeight * (rawWeight / groupSum));

                if (absoluteWeight > 0)
                    options.Add((absoluteWeight, items[i].Creator));
            }
        }
        public Item? CreateRoomItem(IGameSessionService sessionService)
        {
            int roomId = sessionService.RoomCounter;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            // Группа "Прочее"
            AddWeightedGroup(options, roomId, GameBalance.RoomOtherWeight,
                // Относительные веса внутри группы (не в процентах)
                (_ => GameBalance.CalculateNoneRoomWeight(), () => null),
                (_ => GameBalance.CalculateKeyRoomWeight(), () => new Key(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateBagOfCoinsRoomWeight(), () => new BagOfCoins(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateChestRoomWeight(), () => new Chest(this, _enemyFactory, sessionService))
            );

            // Группа "Оружие" 
            AddWeightedGroup(options, roomId, GameBalance.RoomWeaponWeight,
                (r => GameBalance.CalculateRustSwordRoomWeight(r), () => new RustSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronSwordRoomWeight(r), () => new IronSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateSilverSwordRoomWeight(r), () => new SilverSword(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateGlassSwordRoomWeight(), () => new GlassSword(sessionService.NextItemId(), roomId, false)),

                (r => GameBalance.CalculateMagicWandRoomWeight(r), () => new MagicWand(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateRandomWandRoomWeight(r), () => new RandomWand(sessionService.NextItemId(), roomId, false))
            );

            // Группа "Броня"
            AddWeightedGroup(options, roomId, GameBalance.RoomArmorWeight,
                (r => GameBalance.CalculateWoodenBucketRoomWeight(r), () => new WoodenBucket(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateLeatherHelmRoomWeight(r), () => new LeatherHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronHelmRoomWeight(r), () => new IronHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateLeatherVestRoomWeight(r), () => new LeatherVest(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronCuirassRoomWeight(r), () => new IronCuirass(sessionService.NextItemId(), roomId, false))
            );

            // Группа "Зелья"
            AddWeightedGroup(options, roomId, GameBalance.RoomHealWeight,
                (r => GameBalance.CalculateBandageRoomWeight(r), () => new Bandage(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateRegenPotionRoomWeight(r), () => new RegenPotion(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculatePowerPotionRoomWeight(r), () => new PowerPotion(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateRandomPotionRoomWeight(r), () => new RandomPotion(sessionService.NextItemId(), roomId, false))
            );

            return SelectRandom(options);
        }
        public Item? CreateChestItem(IGameSessionService sessionService)
        {
            int roomId = sessionService.RoomCounter;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, GameBalance.ChestOtherWeight,
                (_ => GameBalance.CalculateKeyChestWeight(), () => new Key(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateBagOfCoinsChestWeight(), () => new BagOfCoins(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateMapChestWeight(), () => new Map(sessionService.NextItemId()))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestWeaponWeight,
                (r => GameBalance.CalculateRustSwordChestWeight(r), () => new RustSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronSwordChestWeight(r), () => new IronSword(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateSilverSwordChestWeight(r), () => new SilverSword(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateGlassSwordChestWeight(), () => new GlassSword(sessionService.NextItemId(), roomId, false)),

                (r => GameBalance.CalculateMagicWandChestWeight(r), () => new MagicWand(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateRandomWandChestWeight(), () => new RandomWand(sessionService.NextItemId(), roomId, false))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestArmorWeight,
                (r => GameBalance.CalculateLeatherHelmChestWeight(r), () => new LeatherHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronHelmChestWeight(r), () => new IronHelm(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateLeatherVestChestWeight(r), () => new LeatherVest(sessionService.NextItemId(), roomId, false)),
                (r => GameBalance.CalculateIronCuirassChestWeight(r), () => new IronCuirass(sessionService.NextItemId(), roomId, false))
            );
            AddWeightedGroup(options, roomId, GameBalance.ChestHealWeight,
                (_ => GameBalance.CalculateRegenPotionChestWeight(), () => new RegenPotion(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculatePowerPotionChestWeight(), () => new PowerPotion(sessionService.NextItemId(), roomId, false)),
                (_ => GameBalance.CalculateRandomPotionChestWeight(), () => new RandomPotion(sessionService.NextItemId(), roomId, false))
            );
            return SelectRandom(options);
        }
        public Item? CreateShopItem(IGameSessionService sessionService)
        {
            int roomId = sessionService.RoomCounter;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, GameBalance.ShopOtherWeight,
                (_ => GameBalance.CalculateKeyShopWeight(), () => new Key(sessionService.NextItemId(), roomId)),
                (_ => GameBalance.CalculateMapShopWeight(), () => new Map(sessionService.NextItemId()))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopWeaponWeight,
                (r => GameBalance.CalculateRustSwordShopWeight(r), () => new RustSword(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateIronSwordShopWeight(r), () => new IronSword(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateSilverSwordShopWeight(r), () => new SilverSword(sessionService.NextItemId(), roomId, true)),

                (_ => GameBalance.CalculateMagicWandShopWeight(), () => new MagicWand(sessionService.NextItemId(), roomId, true)),
                (_ => GameBalance.CalculateRandomWandShopWeight(), () => new RandomWand(sessionService.NextItemId(), roomId, true))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopArmorWeight,
                (r => GameBalance.CalculateWoodenBucketShopWeight(r), () => new WoodenBucket(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateLeatherHelmShopWeight(r), () => new LeatherHelm(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateIronHelmShopWeight(r), () => new IronHelm(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateLeatherVestShopWeight(r), () => new LeatherVest(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateIronCuirassShopWeight(r), () => new IronCuirass(sessionService.NextItemId(), roomId, true))
            );
            AddWeightedGroup(options, roomId, GameBalance.ShopHealWeight,
                (r => GameBalance.CalculateBandageShopWeight(r), () => new Bandage(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculateRegenPotionShopWeight(r), () => new RegenPotion(sessionService.NextItemId(), roomId, true)),
                (r => GameBalance.CalculatePowerPotionShopWeight(r), () => new PowerPotion(sessionService.NextItemId(), roomId, true)),
                (_ => GameBalance.CalculateRandomPotionShopWeight(), () => new RandomPotion(sessionService.NextItemId(), roomId, true))
            );
            return SelectRandom(options);
        }
    }
    public class EnemyFactory : IEnemyFactory
    {
        public Skeletor CreateSkeletor(IGameSessionService sessionService) => new Skeletor(sessionService.RoomCounter, sessionService.NextEnemyId());
        public SkeletorArcher CreateSkeletorArcher(IGameSessionService sessionService) => new SkeletorArcher(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Deadman CreateDeadman(IGameSessionService sessionService) => new Deadman(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Ghost CreateGhost(IGameSessionService sessionService) => new Ghost(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Lich CreateLich(IGameSessionService sessionService) => new Lich(sessionService.RoomCounter, sessionService.NextEnemyId());
        public Mimic CreateMimic(IGameSessionService sessionService) => new Mimic(sessionService.RoomCounter, sessionService.NextEnemyId());
    }
    public class MapGenerator : IMapGenerator
    {
        private readonly IRoomFactory _roomFactory;
        private readonly IRoomContentGenerator _roomContentGenerator;
        public MapGenerator(IRoomFactory roomFactory, IRoomContentGenerator roomContentGenerator) 
        {
            _roomFactory = roomFactory;
            _roomContentGenerator = roomContentGenerator;
        }
        public List<Room> Generate(IGameSessionService sessionService)
        {
            var rooms = new List<Room> { _roomFactory.CreateStartRoom() };
            var options = new (int Weight, Func<Room> Create)[]
            {
            (GameBalance.EmptyRoomWeight, () => _roomFactory.CreateEmptyRoom(sessionService.NextRoomNumber())),
            (GameBalance.SmallRoomWeight, () => _roomFactory.CreateSmallRoom(sessionService.NextRoomNumber())),
            (GameBalance.BigRoomWeight, () => _roomFactory.CreateBigRoom(sessionService.NextRoomNumber())),
            (GameBalance.ShopWeight, () => _roomFactory.CreateShopRoom(sessionService.NextRoomNumber()))
            };
            int baseWeightSum = options.Sum(x => x.Weight);

            while (rooms.Last() is not EndRoom)
            {
                int endRoomWeight = sessionService.RoomCounter;
                int totalWeight = baseWeightSum + endRoomWeight;

                int roll = Random.Shared.Next(totalWeight);

                if (roll >= baseWeightSum)
                {
                    Room room = (_roomFactory.CreateEndRoom(sessionService.NextRoomNumber()));
                    _roomContentGenerator.GenerateContent(room,sessionService);
                    rooms.Add(room);
                }
                else
                {
                    int acc = 0;
                    foreach (var (weight, create) in options)
                    {
                        if (roll < acc + weight)
                        {
                            Room room = (create());
                            _roomContentGenerator.GenerateContent(room, sessionService);
                            rooms.Add(room);
                            break;
                        }
                        acc += weight;
                    }
                }
            }

            return rooms;
        }
    }
    public class RoomContentGenerator : IRoomContentGenerator
    {
        private readonly IItemFactory _itemFactory;
        private readonly IEnemyFactory _enemyFactory;
        public RoomContentGenerator(IItemFactory itemFactory, IEnemyFactory enemyFactory)
        {
            _itemFactory = itemFactory;
            _enemyFactory = enemyFactory;
        }
        public void GenerateContent(Room room, IGameSessionService sessionService)
        {
            switch (room)
            {
                case SmallRoom smallRoom: GenerateSmallRoomContent(smallRoom, sessionService); break;
                case BigRoom bigRoom: GenerateBigRoomContent(bigRoom, sessionService); break;
                case Shop shop: GenerateShopContent(shop, sessionService); break;
            }
            if (room is not Shop && room is not StartRoom && room is not EndRoom) GenerateEnemy(room, sessionService);
        }
        private void GenerateSmallRoomContent(SmallRoom room, IGameSessionService sessionService)
        {
            for (int i = 0; i < GameBalance.SmallRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem(sessionService);
                if (item != null) room.AddItem(item);
            }
        }
        private void GenerateBigRoomContent(BigRoom room, IGameSessionService sessionService)
        {
            for (int i = 0; i < GameBalance.BigRoomItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateRoomItem(sessionService);
                if (item != null) room.AddItem(item);
            }
        }
        private void GenerateShopContent(Shop room, IGameSessionService sessionService)
        {
            for (int i = 0; i < GameBalance.ShopItemsAmount; i++)
            {
                Item? item = _itemFactory.CreateShopItem(sessionService);
                if (item == null) continue;
                item.AddStoreMargin();
                room.AddItem(item);
            }
        }
        private void GenerateEnemy(Room room, IGameSessionService sessionService)
        {
            //Логика создания врагов
            //Формирование списка взвешенного выбора
            var options = new List<(int Weight, Func<Enemy?> Create)>
            {
                (GameBalance.CalculateNoneWeight(room.Number),            () => null),
                (GameBalance.CalculateSkeletorWeight(room.Number),        () => _enemyFactory.CreateSkeletor(sessionService)),
                (GameBalance.CalculateSkeletorArcherWeight(room.Number),  () => _enemyFactory.CreateSkeletorArcher(sessionService)),
                (GameBalance.CalculateDeadmanWeight(room.Number),         () => _enemyFactory.CreateDeadman(sessionService)),
                (GameBalance.CalculateGhostWeight(room.Number),           () => _enemyFactory.CreateGhost(sessionService)),
                (GameBalance.CalculateLichWeight(room.Number),            () => _enemyFactory.CreateLich(sessionService)),
            };
            //Выбор
            int weightsSum = options.Sum(x => x.Weight);
            int roll = Random.Shared.Next(weightsSum);
            int accumulated = 0;

            foreach (var option in options)
            {
                if (roll < accumulated + option.Weight)
                {
                    var enemy = option.Create();
                    if (enemy is not null) room.AddEnemy(enemy);
                    break;
                }
                accumulated += option.Weight;
            }
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
        public void CheckItem(Item item, IGameSessionService sessionService)
        {
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is BagOfCoins bagOfCoins) sessionService.AddCoins((int)bagOfCoins.Cost!);
            else if (item is Key) sessionService.AddKeys(1);
            else sessionService.AddItemToInventory(item);
        }
    }
    public class ChestRepository : IChestRepository
    {
        private readonly IGameSessionService _sessionService;

        private IGameInfoRepository _gameInfoRepository;
        private IGetItemById _getItemByIdRepository;
        private ICombatRepository _combatRepository;

        private ICheckItemService _checkItemService;
        public ChestRepository(
            IGameSessionService sessionService,
            IGameInfoRepository gameInfoRepository,
            IGetItemById getItemByIdRepository,
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
            Item item = _getItemByIdRepository.GetItem(chestId, _sessionService.CurrentRoom!.Items);
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
            Item item = _getItemByIdRepository.GetItem(itemId, chest.Items);
            _checkItemService.CheckItem(item, _sessionService);
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
                _checkItemService.CheckItem(item, _sessionService);
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
    public class GetItemById : IGetItemById
    {
        public Item GetItem(int itemId, IEnumerable<Item> items)
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
        private readonly IGetItemById _getItemByIdRepository;
        private readonly IGetEnemyByIdRepository _getEnemyByIdRepository;
        private readonly ICombatRepository _combatRepository;
        private readonly ICheckItemService _checkItemService;
        public RoomControllerRepository(
            IGameSessionService sessionService,
            IGetCurrentRoomRepository getCurrentRoomRepository,
            IChestRepository chestRepository,
            IGameInfoRepository gameInfoRepository,
            IGetRoomByIdRepository getRoomByIdRepository,
            IGetItemById getItemByIdRepository,
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
        public Room GetRoomById(int roomId) => _getRoomByIdRepository.GetRoomById(roomId);
        //public Item GetItemById(int itemId, List<Item> items) => GetItemByIdRepository.GetItemById(itemId, items);
        //public Item GetInventoryItem(int itemId) => InventoryRepository.GetInventoryItem(itemId);
        //public List<Item> GetInventoryItems(List<int> itemIds) => InventoryRepository.GetInventoryItems(itemIds);
        public GameInfoDTO GetGameInfo() => _gameInfoRepository.GetGameInfo();
    }
}