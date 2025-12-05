using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System;
using System.Linq.Expressions;
using System.Numerics;
using TextGame;
using static System.Net.Mime.MediaTypeNames;

namespace TextGame
{
    public class GameObject
    {
        public string Name { get; set; } = "НЕИЗВЕСТНО";
        public string Description { get; set; } = "НЕОПИСУЕМО!";
    }
    #region Room
    public class Room : GameObject
    {
        private readonly IEnemyFactory? _enemyFactory;
        public int Number { get; init; }
        private readonly List<Item> _items = new();
        public IReadOnlyList<Item> Items => _items;
        private readonly List<Enemy> _enemies = new();
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public bool IsDiscovered { get; set; } = false;
        public bool IsSearched { get; set; } = false;

        public Room(string name, string description, int number, IEnemyFactory? enemyFactory = null)
        {
            Name = name;
            Description = description;
            Number = number;
            _enemyFactory = enemyFactory;
        }
        public void AddItem(Item item) => _items.Add(item);
        public void RemoveItem(Item item)
        {
            if (_items.Contains(item)) _items.Remove(item);
            else throw new NullItemIdException();
        }
        public void AddEnemy(Enemy enemy) => _enemies.Add(enemy);
        public void RemoveEnemy(Enemy enemy)
        {

            if (_enemies.Contains(enemy)) _enemies.Remove(enemy);
            else throw new NullEnemyIdException();
        }
        public List<Item> Search()
        {
            IsSearched = true;
            return _items;
        }
        public void CreateEnemy(IGameSessionService sessionService)
        {
            if (_enemyFactory == null) return;
            int roomId = sessionService.RoomCounter;
            //Логика создания врагов

            //Формирование списка взвешенного выбора
            var options = new List<(int Weight, Func<Enemy?> Create)>
            {
                (GameBalance.CalculateNoneWeight(roomId),            () => null),
                (GameBalance.CalculateSkeletorWeight(roomId),        () => _enemyFactory.CreateSkeletor(sessionService)),
                (GameBalance.CalculateSkeletorArcherWeight(roomId),  () => _enemyFactory.CreateSkeletorArcher(sessionService)),
                (GameBalance.CalculateDeadmanWeight(roomId),         () => _enemyFactory.CreateDeadman(sessionService)),
                (GameBalance.CalculateGhostWeight(roomId),           () => _enemyFactory.CreateGhost(sessionService)),
                (GameBalance.CalculateLichWeight(roomId),            () => _enemyFactory.CreateLich(sessionService)),
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
                    if (enemy is not null) AddEnemy(enemy);
                    break;
                }
                accumulated += option.Weight;
            }
        }
    }
    public class StartRoom : Room
    {
        public StartRoom()
            : base("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую Вы сюда провалились.", 0)
        {
            IsDiscovered = true;
        }
    }
    public class EndRoom : Room
    {
        public EndRoom(IGameSessionService sessionService)
            : base("ВЫХОД", "Выход наружу. Свобода.", sessionService.NextRoomNumber()) { }
    }
    public class EmptyRoom : Room
    {
        public EmptyRoom(IEnemyFactory enemyFactory, IGameSessionService sessionService)
            : base("ПУСТАЯ КОМНАТА", "Ничего интересного.", sessionService.NextRoomNumber())
        {
            CreateEnemy(sessionService);
        }
    }
    public class SmallRoom : Room
    {
        public SmallRoom(IItemFactory itemFactory, IEnemyFactory enemyFactory, IGameSessionService sessionService)
            : base("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", sessionService.NextRoomNumber(), enemyFactory)
        {
            for (int i = 0; i < GameBalance.SmallRoomItemsAmount; i++)
            {
                Item? item = itemFactory.CreateRoomItem(sessionService);
                if (item != null) AddItem(item);
            }
            CreateEnemy(sessionService);
        }
    }
    public class BigRoom : Room
    {
        public BigRoom(IItemFactory itemFactory, IEnemyFactory enemyFactory, IGameSessionService sessionService)
            : base("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", sessionService.NextRoomNumber(), enemyFactory)
        {
            for (int i = 0; i < GameBalance.BigRoomItemsAmount; i++)
            {
                Item? item = itemFactory.CreateRoomItem(sessionService);
                if (item != null) AddItem(item);
            }
            CreateEnemy(sessionService);
        }
    }
    public class Shop : Room
    {
        public Shop(IItemFactory itemFactory, IGameSessionService sessionService)
            : base("МАГАЗИН", "Здесь мутный торгаш продаёт своё добро.", sessionService.NextRoomNumber())
        {
            for (int i = 0; i < GameBalance.ShopItemsAmount; i++)
            {
                Item? item = itemFactory.CreateShopItem(sessionService);
                if (item == null) continue;
                item.AddStoreMargin();
                AddItem(item);
            }
        }
    }

    public interface IRoomFactory
    {
        StartRoom CreateStartRoom();
        EmptyRoom CreateEmptyRoom(IGameSessionService sessionService);
        SmallRoom CreateSmallRoom(IGameSessionService sessionService);
        BigRoom CreateBigRoom(IGameSessionService sessionService);
        Shop CreateShopRoom(IGameSessionService sessionService);
        EndRoom CreateEndRoom(IGameSessionService sessionService);
    }

    public class RoomFactory : IRoomFactory
    {
        private readonly IItemFactory _itemFactory;
        private readonly IEnemyFactory _enemyFactory;

        public RoomFactory(IItemFactory itemFactory, IEnemyFactory enemyFactory)
        {
            _itemFactory = itemFactory;
            _enemyFactory = enemyFactory;
        }
        public StartRoom CreateStartRoom() => new StartRoom();
        public EmptyRoom CreateEmptyRoom(IGameSessionService sessionService) => new EmptyRoom(_enemyFactory, sessionService);
        public SmallRoom CreateSmallRoom(IGameSessionService sessionService) => new SmallRoom(_itemFactory, _enemyFactory, sessionService);
        public BigRoom CreateBigRoom(IGameSessionService sessionService) => new BigRoom(_itemFactory, _enemyFactory, sessionService);
        public Shop CreateShopRoom(IGameSessionService sessionService) => new Shop(_itemFactory, sessionService);
        public EndRoom CreateEndRoom(IGameSessionService sessionService) => new EndRoom(sessionService);
    }
    #endregion
    #region Item
    public class Item : GameObject
    {
        public int? Id { get; init; }
        public int? Cost { get; protected set; }
        public bool IsCarryable { get; init; }
        public Item(string name, string description, int? id, bool isCarryable)
        {
            Name = name;
            Description = description;
            Id = id;
            IsCarryable = isCarryable;
        }
        public virtual void AddStoreMargin() => Cost = (int)(Cost! * GameBalance.StoreMargin);
    }
    public class Key : Item
    {
        public Key(int itemId, int roomId)
            : base("КЛЮЧ", "Непрочный продолговатый кусок металла. Что-то открывает.", itemId, true)
        {
            Cost = (int)(GameBalance.KeyBaseCost * GameBalance.ApplyGain(roomId));
        }
    }
    public class BagOfCoins : Item
    {

        public BagOfCoins(int itemId, int roomId)
            : base("МЕШОЧЕК С МОНЕТАМИ", "Потрёпанный кусок ткани с разными монетами внутри.", itemId, true)
        {
            var (min, max) = GameBalance.ApplySpread(GameBalance.BagOfCoinsBaseCost, roomId);
            Cost = Random.Shared.Next(min, max + 1);
        }
    }
    public class Chest : Item
    {
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; } = true;
        public Mimic? Mimic { get; private set; }
        public readonly List<Item> _items = new();
        public IReadOnlyList<Item> Items => _items.AsReadOnly();

        public Chest(IItemFactory itemFactory, IEnemyFactory enemyFactory, IGameSessionService sessionService)
            : base("СУНДУК", "Хранит предметы. Может оказаться мимиком.", sessionService.NextItemId(), false)
        {
            Cost = null;
            IsLocked = Random.Shared.Next(GameBalance.ChestDivider) < GameBalance.LockedProbabilityDenominator;
            Mimic = Random.Shared.Next(GameBalance.ChestDivider) < GameBalance.MimicProbabilityDenominator ? enemyFactory.CreateMimic(sessionService) : null;
            var itemsInChest = Random.Shared.Next(GameBalance.MinChestItemsAmount, GameBalance.MaxChestItemsAmount + 1);
            for (int i = 0; i < itemsInChest; i++)
            {
                Item? item = itemFactory.CreateChestItem(sessionService);
                if (item != null) AddItem(item);
            }
        }
        public void AddItem(Item item) => _items.Add(item);
        public void RemoveItem(Item item)
        {
            if (_items.Contains(item)) _items.Remove(item);
            else throw new NullItemIdException();
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public IEnumerable<Item> Search() => Items;
        public void KillMimic()
        {
            Mimic = null;
            Name = "МЁРТВЫЙ МИМИК";
            Description = "Мёртвый сундук с руками и зубами. Интересно, что у него внутри.";
            IsLocked = false;
            IsClosed = false;
        }
    }
    public class Map : Item
    {
        public Map(int itemId)
            : base("КАРТА", "Содержит знания о строении подземелья.", itemId, true)
        {
            Cost = GameBalance.MapBaseCost;
        }
    }
    #region Heal
    public abstract class Heal : Item
    {
        public int? MaxHealthBoost { get; protected set; } = 0;
        public int? CurrentHealthBoost { get; protected set; } = 0;

        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public Heal(string name, string description, int id, int roomId, bool fromShop, int? maxHealthBoost, int? currentHealthBoost) : base(name, description, id, true)
        {
            _roomId = roomId;
            _fromShop = fromShop;

            Initialize(maxHealthBoost, currentHealthBoost);
        }
        protected virtual void Initialize(int? maxHealthBoost, int? currentHealthBoost)
        {
            Cost = 1;
            if (maxHealthBoost is null) MaxHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.ApplySpread((int)maxHealthBoost!, _roomId);
                MaxHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) GameBalance.ApplyShopMultiplier((int)MaxHealthBoost!);
                Cost += (int)(MaxHealthBoost * GameBalance.MaxHealthCostMultiplier);
            }

            if (currentHealthBoost is null) CurrentHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.ApplySpread((int)currentHealthBoost!, _roomId);
                CurrentHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) GameBalance.ApplyShopMultiplier((int)CurrentHealthBoost!);
                Cost += (int)(CurrentHealthBoost * GameBalance.CurrentHealthCostMultiplier);
            }
        }
        public virtual void Use(IGameSessionService sessionService)
        {
            sessionService.AddMaxHealth((int)MaxHealthBoost!);
            sessionService.AddCurrentHealth((int)CurrentHealthBoost!);
        }
    }
    public class Bandage : Heal
    {
        public Bandage(int itemId, int roomId, bool fromShop)
            : base("ПОВЯЗКА", "Менее грязная тряпка, из тех, что здесь обычно встречаются.", itemId, roomId, fromShop, GameBalance.BandageBaseMaxHealthBoost, GameBalance.BandageBaseCurrentHealthBoost) { }
    }
    public class RegenPotion : Heal
    {
        public RegenPotion(int itemId, int roomId, bool fromShop)
            : base("ЗЕЛЬЕ РЕГЕНЕРАЦИИ", "Пыльный бутылёк с субстанцией тёмного цвета.", itemId, roomId, fromShop, GameBalance.RegenPotionBaseMaxHealthBoost, GameBalance.RegenPotionBaseCurrentHealthBoost) { }
    }
    public class PowerPotion : Heal
    {
        public PowerPotion(int itemId, int roomId, bool fromShop)
            : base("ЗЕЛЬЕ СИЛЫ", "Не только восстанавливает силы, но и придаёт новых.", itemId, roomId, fromShop, GameBalance.PowerPotionBaseMaxHealthBoost, GameBalance.PowerPotionBaseCurrentHealthBoost) { }
    }
    public class RandomPotion : Heal
    {
        public RandomPotion(int itemId, int roomId, bool fromShop)
            : base("НЕИЗВЕСТНОЕ ЗЕЛЬЕ", "Пробирка с жижей непонятного цвета.", itemId, roomId, fromShop, null, null) { }
        public override void Use(IGameSessionService sessionService)
        {
            double maxHealthFloor = GameBalance.RandomPotionBaseMaxHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadFloor;
            double maxHealthCeiling = GameBalance.RandomPotionBaseMaxHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadCeiling;
            double currentHealthFloor = GameBalance.RandomPotionBaseCurrentHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadFloor;
            double currentHealthCeiling = GameBalance.RandomPotionBaseCurrentHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadCeiling;
            if (_fromShop)
            {
                maxHealthFloor *= 1 / GameBalance.ShopMultiplier;
                maxHealthCeiling *= GameBalance.ShopMultiplier;
                currentHealthFloor *= 1 / GameBalance.ShopMultiplier;
                currentHealthCeiling *= GameBalance.ShopMultiplier;
            }
            MaxHealthBoost = Random.Shared.Next((int)maxHealthFloor, (int)currentHealthCeiling + 1);
            CurrentHealthBoost = Random.Shared.Next((int)currentHealthFloor, (int)currentHealthCeiling + 1);
            base.Use(sessionService);
        }
    }
    #endregion
    #region Equipment
    public abstract class Equipment : Item
    {
        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public int? Durability { get; protected set; }
        public Equipment(string name, string description, int? id, int? durability, int roomId, bool fromShop) : base(name, description, id, true)
        {
            _roomId = roomId;
            _fromShop = fromShop;

            Durability = durability;
        }
    }
    #region Weapon
    public abstract class Weapon : Equipment
    {
        public int Damage { get; protected set; }

        public Weapon(string name, string description, int? id, int? durability, int damage, int roomId, bool fromShop)
            : base(name, description, id, durability, roomId, fromShop)
        {
            Damage = damage;
        }
        public abstract int Attack(IGameSessionService sessionService);
    }
    public class Fists : Weapon
    {
        public static readonly Fists DefaultFists = new Fists();

        public Fists() : base("КУЛАКИ", "То, что есть (почти) у каждого. Базовое оружие самозащиты. Может быть больно.", null, null, GameBalance.FistsBaseDamage, 0, false) { }
        public override int Attack(IGameSessionService sessionService)
        {
            var (min, max) = GameBalance.ApplySpread(GameBalance.FistsBaseDamage, sessionService.CurrentRoom!.Number);
            int damage = Random.Shared.Next(min, max + 1);
            if (Random.Shared.Next(GameBalance.FistSelfHarmProbabilityDivider) == 0)
                sessionService.AddCurrentHealth(-(int)(damage / GameBalance.FistSelfHarmDivider));
            return damage;
        }
    }
    public abstract class Sword : Weapon
    {
        public Sword(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damage) : base(name, description, itemId, null, 0, roomId, fromShop)
        {
            Initialize(durability, damage);
        }
        protected void Initialize(int durability, int damage)
        {
            var (minDurability, maxDurability) = GameBalance.ApplySpread(durability, _roomId);
            Durability = Random.Shared.Next(minDurability, maxDurability + 1);
            var (minDamage, maxDamage) = GameBalance.ApplySpread(damage, _roomId);
            Damage = Random.Shared.Next(minDamage, maxDamage + 1);
            if (_fromShop)
            {
                Durability = GameBalance.ApplyShopMultiplier((int)Durability!);
                Damage = GameBalance.ApplyShopMultiplier(Damage);
            }
            CalculateCost();
        }
        public override int Attack(IGameSessionService sessionService)
        {
            Durability--;
            CalculateCost();
            if (Durability <= 0) BreakDown(sessionService);
            return Damage;
        }
        public void BreakDown(IGameSessionService sessionService)
        {
            sessionService.RemoveWeapon();
        }
        private void CalculateCost()
        {
            Cost = GameBalance.CalculateSwordCost((int)Durability!, Damage);
        }
    }
    public class RustSword : Sword
    {
        public RustSword(int itemId, int roomId, bool fromShop)
            : base("РЖАВЫЙ МЕЧ", "Очень старый меч. Лучше, чем ничего.", itemId, roomId, fromShop, GameBalance.RustSwordBaseDurability, GameBalance.RustSwordBaseDamage) { }
    }
    public class IronSword : Sword
    {
        public IronSword(int itemId, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНЫЙ МЕЧ", "Добротное оружие воина. На лезвии оттиск \"304\".Номер, наверное.", itemId, roomId, fromShop, GameBalance.IronSwordBaseDurability, GameBalance.IronSwordBaseDamage) { }
    }
    public class SilverSword : Sword
    {
        public SilverSword(int itemId, int roomId, bool fromShop)
            : base("СЕРЕБРЯНЫЙ МЕЧ", "Редкий меч из особого серебряного сплава. Эффективный, но менее прочный.", itemId, roomId, fromShop, GameBalance.SilverSwordBaseDurability, GameBalance.SilverSwordBaseDamage) { }
    }
    public class GlassSword : Sword
    {
        public GlassSword(int itemId, int roomId, bool fromShop)
            : base("СТЕКЛЯННЫЙ МЕЧ", "Скорее объект искусства, чем оружие. Очень хрупкий, но невероятно сильный.", itemId, roomId, fromShop, GameBalance.GlassSwordBaseDurability, GameBalance.GlassSwordBaseDamage) { }
    }
    public abstract class Wand : Weapon
    {
        public Wand(string name, string description, int itemId, int roomId, bool fromShop, int damage) : base(name, description, itemId, null, 0, roomId, fromShop)
        {
            Initialize(damage);
        }
        protected void Initialize(int damage)
        {
            var (min, max) = GameBalance.ApplySpread(damage, _roomId);
            Damage = Random.Shared.Next(min, max + 1);
            if (_fromShop) Damage = GameBalance.ApplyShopMultiplier(Damage);
            Cost = GameBalance.CalculateWandCost(Damage);
        }
        public override int Attack(IGameSessionService sessionService)
        {
            return Damage;
        }
    }
    public class MagicWand : Wand
    {

        public MagicWand(int itemId, int roomId, bool fromShop)
            : base("ВОЛШЕБНЫЙ ЖЕЗЛ", "Простое магическое оружие. Может использовать каждый.", itemId, roomId, fromShop, GameBalance.MagicWandBaseDamage) { }
    }
    public class RandomWand : Wand
    {
        public RandomWand(int itemId, int roomId, bool fromShop)
            : base("ЖЕЗЛ СЛУЧАЙНОСТЕЙ", "Странное магическое оружие. Становится сильнее со временем.", itemId, roomId, fromShop, GameBalance.RandomWandBaseDamage) { }
        public override int Attack(IGameSessionService sessionService)
        {
            int damage = (int)(Damage * GameBalance.ApplyGain(_roomId));
            return Random.Shared.Next(damage + 1);
        }
    }

    #endregion
    #region Armor
    public abstract class Armor : Equipment
    {
        public int DamageBlock { get; protected set; }
        public Armor(string name, string description, int id, int roomId, bool fromShop, int durability, int damageBlock) : base(name, description, id, durability, roomId, fromShop)
        {
            Initialize(durability, damageBlock);
        }
        protected void Initialize(int durability, int damageBlock)
        {
            Durability = Random.Shared.Next(
                (int)(durability * GameBalance.SpreadFloor),
                (int)(durability * GameBalance.SpreadCeiling + 1));
            var (min, max) = GameBalance.ApplySpread(damageBlock, _roomId);
            DamageBlock = Random.Shared.Next(min, max + 1);
            if (_fromShop)
            {
                Durability = GameBalance.ApplyShopMultiplier((int)Durability!);
                DamageBlock = GameBalance.ApplyShopMultiplier(DamageBlock);
            }
            CalculateCost();
        }
        public int Block(IGameSessionService sessionService)
        {
            Durability--;
            CalculateCost();
            if (Durability <= 0) BreakDown(sessionService);
            return DamageBlock;
        }
        protected abstract void BreakDown(IGameSessionService sessionService);
        private void CalculateCost()
        {
            Cost = GameBalance.CalculateArmorCost((int)Durability!, DamageBlock);
        }
    }
    public abstract class Helm : Armor
    {
        public Helm(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damageBlock)
            : base(name, description, itemId, roomId, fromShop, durability, damageBlock) { }
        protected override void BreakDown(IGameSessionService sessionService)
        {
            sessionService.RemoveHelm();
        }
    }
    public class WoodenBucket : Helm
    {

        public WoodenBucket(int itemId, int roomId, bool fromShop)
            : base("ДЕРЕВЯННОЕ ВЕДРО", "Старое дырявое ведро. Кто в своём уме наденет его на голову?", itemId, roomId, fromShop, GameBalance.WoodenBucketBaseDurability, GameBalance.WoodenBucketBaseDamageBlock) { }
    }
    public class LeatherHelm : Helm
    {
        public LeatherHelm(int itemId, int roomId, bool fromShop)
            : base("КОЖАННЫЙ ШЛЕМ", "Изысканный чёрный шлем мастера подземелия.", itemId, roomId, fromShop, GameBalance.LeatherHelmBaseDurability, GameBalance.LeatherHelmBaseDamageBlock) { }
    }
    public class IronHelm : Helm
    {
        public IronHelm(int itemId, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНЫЙ ШЛЕМ", "Крепкий шлем из качественного металла.", itemId, roomId, fromShop, GameBalance.IronHelmBaseDurability, GameBalance.IronHelmBaseDamageBlock) { }
    }
    public abstract class Chestplate : Armor
    {
        public Chestplate(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damageBlock)
            : base(name, description, itemId, roomId, fromShop, durability, damageBlock) { }
        protected override void BreakDown(IGameSessionService sessionService)
        {
            sessionService.RemoveChestplate();
        }
    }
    public class LeatherVest : Chestplate
    {
        public LeatherVest(int itemId, int roomId, bool fromShop)
            : base("КОЖАННАЯ КУРТКА", "Лёгкая куртка из плотной кожи.", itemId, roomId, fromShop, GameBalance.LeatherVestBaseDurability, GameBalance.LeatherVestBaseDamageBlock) { }

    }
    public class IronCuirass : Chestplate
    {
        public IronCuirass(int itemId, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНАЯ КИРАСА", "Тяжёлая и прочная. Имеет небольшой оттиск \"304\" на внутренней части.", itemId, roomId, fromShop, GameBalance.IronCuirassBaseDurability, GameBalance.IronCuirassBaseDamageBlock) { }
    }
    #endregion
    #endregion
    #region IdFactory
    public interface IIdFactory
    {
        int Id();
        void Reset();
    }
    #endregion
    #region ItemFactory
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
    #endregion
    #endregion
    #region Enemy
}
public abstract class Enemy : GameObject
{
    public int Id { get; protected set; }
    public int Health { get; protected set; } = 0;
    public int Damage { get; protected set; } = 0;
    public int DamageBlock { get; protected set; } = 0;

    private readonly int _roomId;

    public Enemy(string name, string description, int roomId, int id, int health, int damage, int damageBlock)
    {
        _roomId = roomId;

        Name = name;
        Description = description;
        Id = id;
        Initialize(health, damage, damageBlock);
    }
    public virtual void Initialize(int health, int damage, int damageBlock)
    {
        var (minHealth, maxHealth) = GameBalance.ApplySpread(health, _roomId);
        Health = Random.Shared.Next(minHealth, maxHealth + 1);

        var (minDamage, maxDamage) = GameBalance.ApplySpread(damage, _roomId);
        Damage = Random.Shared.Next(minDamage, maxDamage + 1);

        var (minDamageBlock, maxDamageBlock) = GameBalance.ApplySpread(damageBlock, _roomId);
        DamageBlock = Random.Shared.Next(minDamageBlock, maxDamageBlock + 1);
    }
    public virtual int Attack()
    {
        return Damage;
    }
    public virtual int GetDamage(int damage, Room? room = null)
    {
        if (damage > DamageBlock) Health -= damage - DamageBlock;
        return Health;
    }
}
public class Skeletor : Enemy
{

    public Skeletor(int roomId, int enemyId)
        : base("СКЕЛЕТОР", "Чей-то скелет, наделёный возможностью двигаться.", roomId, enemyId, GameBalance.SkeletorBaseHealth, GameBalance.SkeletorBaseDamage, GameBalance.SkeletorBaseDamageBlock) { }
}
public class SkeletorArcher : Enemy
{
    public SkeletorArcher(int roomId, int enemyId)
        : base("СКЕЛЕТОР-ЛУЧНИК", "Из тех, кто при жизни умел обращаться с луком.", roomId, enemyId, GameBalance.SkeletorArcherBaseHealth, GameBalance.SkeletorArcherBaseDamage, GameBalance.SkeletorArcherBaseDamageBlock) { }
}
public class Deadman : Enemy
{
    public Deadman(int roomId, int enemyId)
        : base("МЕРТВЯК", "Мёртвое полуразложившееся тело. Источник жуткого смрада.", roomId, enemyId, GameBalance.DeadmanBaseHealth, GameBalance.DeadmanBaseDamage, GameBalance.DeadmanBaseDamageBlock) { }
}
public class Ghost : Enemy
{
    public Ghost(int roomId, int enemyId)
        : base("ПРИЗРАК", "Злой полуматериальный дух. Попробуй попади.", roomId, enemyId, GameBalance.GhostBaseHealth, GameBalance.GlassSwordBaseDamage, GameBalance.GhostBaseDamageBlock) { }
    public override int GetDamage(int damage, Room? room = null)
    {
        if (Random.Shared.Next(GameBalance.GhostHitDivider) == 0) Health -= damage;
        return Health;
    }
}
public class Lich : Enemy
{
    public Lich(int roomId, int enemyId)
        : base("ЛИЧ", "Тебя ждёт вечный параЛИЧ. Ха-ха.", roomId, enemyId, GameBalance.LichBaseHealth, GameBalance.LichBaseDamage, GameBalance.LichBaseDamageBlock) { }
}
public class Mimic : Enemy
{
    public Mimic(int roomId, int enemyId)
        : base("МИМИК", "Подлый монстр, изменяющий свой облик для охоты на неосторожных попаданцев.", roomId, enemyId, GameBalance.MimicBaseHealth, GameBalance.MimicBaseDamage, GameBalance.MimicBaseDamageBlock) { }
}

public interface IEnemyFactory
{
    Skeletor CreateSkeletor(IGameSessionService sessionService);
    SkeletorArcher CreateSkeletorArcher(IGameSessionService sessionService);
    Deadman CreateDeadman(IGameSessionService sessionService);
    Ghost CreateGhost(IGameSessionService sessionService);
    Lich CreateLich(IGameSessionService sessionService);
    Mimic CreateMimic(IGameSessionService sessionService);
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
#endregion