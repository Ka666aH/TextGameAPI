using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System;
using System.Linq.Expressions;
using System.Numerics;
using TextGame;
using static System.Net.Mime.MediaTypeNames;

namespace TextGame
{
    public class GameBalance
    {
        public const double SpreadFloor = 0.8;
        public const double SpreadCeiling = 1.2;
        public const double ShopMultiplier = 1.2;
        private const double _gain = 0.01;

        public static double ApplyGain(int roomId) => 1 + roomId * _gain;
        public static (int min, int max) ApplySpread(int baseValue, int roomId)
        {
            int minValue = (int)(baseValue * ApplyGain(roomId) * SpreadFloor);
            int maxValue = (int)(baseValue * ApplyGain(roomId) * SpreadCeiling);
            return (minValue, maxValue);
        }
        public static int ApplyShopMultiplier(int baseValue) => (int)(baseValue * ShopMultiplier);

    }
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
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public bool IsDiscovered { get; set; } = false;
        public bool IsSearched { get; set; } = false;

        private Random _random = Random.Shared;

        public Room(string name, string description, int number, IEnemyFactory? enemyFactory = null)
        {
            Name = name;
            Description = description;
            Number = number;
            _enemyFactory = enemyFactory;
        }
        public void CreateEnemy(int roomId)
        {
            if (_enemyFactory == null) return;
            //Логика создания врагов

            //Расчёт весов (вероятности) появления врагов
            int noneWeight = roomId < 100 ? (int)(-0.4 * roomId + 60) : 20;
            int skeletorWeight = roomId < 50 ? (int)-0.48 * roomId + 24 : 0;
            int skeletorArcherWeight = roomId switch
            {
                < 10 => (int)(0.3 * roomId + 12),
                >= 10 and < 65 => (int)(-0.27778 * (roomId - 10) + 15),
                _ => 0
            };
            int deadmanWeight = roomId switch
            {
                <= 100 => (int)(0.2 * roomId + 4),
                > 100 and <= 200 => (int)(-0.08 * (roomId - 100) + 24),
                _ => 20
            };
            int ghostWeight = roomId switch
            {
                <= 100 => (int)(0.32 * roomId),
                > 100 and <= 200 => (int)(-0.08 * (roomId - 100) + 32),
                _ => 30
            };
            int lichWeight = roomId switch
            {
                <= 10 => 0,
                > 10 and <= 200 => (int)(0.21053 * (roomId - 10)),
                _ => 50
            };
            //Формирование списка взвешенного выбора
            var options = new List<(int Weight, Action Create)>
            {
                (noneWeight,            ()=>{}),
                (skeletorWeight,        () => Enemies.Add(_enemyFactory.CreateSkeletor(roomId))),
                (skeletorArcherWeight,  () => Enemies.Add(_enemyFactory.CreateSkeletorArcher(roomId))),
                (deadmanWeight,         () => Enemies.Add(_enemyFactory.CreateDeadman(roomId))),
                (ghostWeight,           () => Enemies.Add(_enemyFactory.CreateGhost(roomId))),
                (lichWeight,            () => Enemies.Add(_enemyFactory.CreateLich(roomId))),
            };
            //Выбор
            int weightsSum = options.Sum(x => x.Weight);
            int roll = _random.Next(weightsSum);
            int accumulated = 0;

            foreach (var option in options)
            {
                if (roll < accumulated + option.Weight)
                {
                    option.Create();
                    break;
                }
                accumulated += option.Weight;
            }
        }
    }
    public class StartRoom : Room
    {
        public StartRoom() : base("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую Вы сюда провалились.", 0)
        {
            IsDiscovered = true;
        }
    }
    public class EndRoom : Room
    {
        public EndRoom(IRoomNumberFactory roomNumberFactory) : base("ВЫХОД", "Выход наружу. Свобода.", roomNumberFactory.GetRoomNumber()) { }
    }
    public class EmptyRoom : Room
    {
        public EmptyRoom(IRoomNumberFactory roomNumberFactory, IEnemyFactory enemyFactory) : base("ПУСТАЯ КОМНАТА", "Ничего интересного.", roomNumberFactory.GetRoomNumber(), enemyFactory)
        {
            CreateEnemy(Number);
        }
    }
    public class SmallRoom : Room
    {
        public SmallRoom(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory) : base("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", roomNumberFactory.GetRoomNumber(), enemyFactory)
        {
            Item? item = itemFactory.CreateRoomItem(Number);
            if (item != null) Items.Add(item);
            CreateEnemy(Number);
        }
    }
    public class BigRoom : Room
    {
        private const int _itemsAmount = 3;
        public BigRoom(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory) : base("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", roomNumberFactory.GetRoomNumber(), enemyFactory)
        {
            for (int i = 0; i < _itemsAmount; i++)
            {
                Item? item = itemFactory.CreateRoomItem(Number);
                if (item != null) Items.Add(item);
            }
            CreateEnemy(Number);
        }
    }
    public class Shop : Room
    {
        private const int _itemsAmount = 5;
        public Shop(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory) : base("МАГАЗИН", "Здесь мутный торгаш продаёт своё добро.", roomNumberFactory.GetRoomNumber())
        {
            for (int i = 0; i < _itemsAmount; i++)
            {
                Item? item = itemFactory.CreateShopItem(Number);
                if (item == null) continue;
                item.AddStoreMargin();
                Items.Add(item);
            }
        }
    }
    public interface IRoomNumberFactory
    {
        int Number { get; }
        int GetRoomNumber();
        void Reset();
    }
    public class RoomNumberFactory : IRoomNumberFactory
    {
        private int _number = 0;

        public int Number => Volatile.Read(ref _number);
        public int GetRoomNumber()
        {
            Interlocked.Increment(ref _number);
            return _number;
        }
        public void Reset() => _number = 0;
    }

    public interface IRoomFactory
    {
        Room CreateRoom();
    }

    public class RoomFactory : IRoomFactory
    {
        private readonly IRoomNumberFactory _roomNumberFactory;
        private readonly IItemFactory _itemFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly Random _random = Random.Shared;

        private const int _emptyRoomMax = 3000;
        private const int _smallRoomMax = 7000;
        private const int _bigRoomMax = 9000;
        private const int _shopMax = 10000;

        public RoomFactory(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory)
        {
            _roomNumberFactory = roomNumberFactory;
            _itemFactory = itemFactory;
            _enemyFactory = enemyFactory;
        }

        public Room CreateRoom()
        {
            var roomTypeNumber = _random.Next(_shopMax + _roomNumberFactory.Number);
            return roomTypeNumber switch
            {
                < _emptyRoomMax => new EmptyRoom(_roomNumberFactory, _enemyFactory),
                >= _emptyRoomMax and < _smallRoomMax => new SmallRoom(_roomNumberFactory, _itemFactory, _enemyFactory),
                >= _smallRoomMax and < _bigRoomMax => new BigRoom(_roomNumberFactory, _itemFactory, _enemyFactory),
                >= _bigRoomMax and < _shopMax => new Shop(_roomNumberFactory, _itemFactory),
                _ => new EndRoom(_roomNumberFactory),
            };
        }
    }
    #endregion
    #region Item
    public class Item : GameObject
    {
        public int? Id { get; init; }
        public int? Cost { get; protected set; }
        public bool IsCarryable { get; init; }

        protected Random _random = Random.Shared;
        public Item(string name, string description, int? id, bool isCarryable)
        {
            Name = name;
            Description = description;
            Id = id;
            IsCarryable = isCarryable;
        }
        public virtual void AddStoreMargin() => Cost *= 2;
    }
    public class Key : Item
    {
        private const int _baseCost = 35;
        public Key(IItemIdFactory itemIdFactory, int roomId) : base("КЛЮЧ", "Непрочный продолговатый кусок металла. Что-то открывает.", itemIdFactory!.Id(), true)
        {
            Cost = (int)(_baseCost * GameBalance.ApplyGain(roomId));
        }
    }
    public class BagOfCoins : Item
    {
        private const int _baseCost = 11;
        public BagOfCoins(IItemIdFactory itemIdFactory, int roomId) : base("МЕШОЧЕК С МОНЕТАМИ", "Потрёпанный кусок ткани с разными монетами внутри.", itemIdFactory!.Id(), true)
        {
            var (min, max) = GameBalance.ApplySpread(_baseCost, roomId);
            Cost = _random.Next(min, max + 1);
        }
    }
    public class Chest : Item
    {
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; } = true;
        public Mimic? Mimic { get; private set; }
        public List<Item> Items { get; set; }

        private const int _minChestItemsAmount = 1;
        private const int _maxChestItemsAmount = 3;

        private const int _lockedProbabilityDenominator = 75;
        private const int _mimicProbabilityDenominator = 50;
        private const int _chestDivider = 100;

        public Chest(IItemIdFactory itemIdFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory, int roomId) : base("СУНДУК", "Хранит предметы. Может оказаться мимиком.", itemIdFactory!.Id(), false)
        {
            Cost = null;
            IsLocked = _random.Next(_chestDivider) < _lockedProbabilityDenominator;
            Mimic = _random.Next(_chestDivider) < _mimicProbabilityDenominator ? enemyFactory.CreateMimic(roomId) : null;
            Items = new List<Item>();
            var itemsInChest = _random.Next(_minChestItemsAmount, _maxChestItemsAmount + 1);
            for (int i = 0; i < itemsInChest; i++)
            {
                Item? item = itemFactory.CreateChestItem(roomId);
                if (item != null) Items.Add(item);
            }
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public List<Item> Search() => Items;
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
        public Map(IItemIdFactory itemIdFactory) : base("КАРТА", "Содержит знания о строении подземелья.", itemIdFactory!.Id(), true)
        {
            Cost = 100;
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
                MaxHealthBoost = _random.Next(min, max + 1);
                if (_fromShop) GameBalance.ApplyShopMultiplier((int)MaxHealthBoost!);
                Cost += MaxHealthBoost * 2;
            }

            if (currentHealthBoost is null) CurrentHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.ApplySpread((int)currentHealthBoost!, _roomId);
                CurrentHealthBoost = _random.Next(min, max + 1);
                if (_fromShop) GameBalance.ApplyShopMultiplier((int)CurrentHealthBoost!);
                Cost += CurrentHealthBoost;
            }
        }
        public virtual void Use(GameSession gameSession)
        {
            if (MaxHealthBoost != 0)
            {
                gameSession.MaxHealth += (int)MaxHealthBoost!;
                gameSession.CurrentHealth += (int)MaxHealthBoost!;
                //if (gameSession.MaxHealth <= 0) gameSession.MaxHealth = 1;
            }
            if (CurrentHealthBoost != 0)
            {
                if ((gameSession.CurrentHealth + (int)CurrentHealthBoost!) >= gameSession.MaxHealth)
                    gameSession.CurrentHealth = gameSession.MaxHealth;
                else
                    gameSession.CurrentHealth += (int)CurrentHealthBoost!;
            }
        }
    }
    public class Bandage : Heal
    {
        private const int _baseMaxHealthBoost = 0;
        private const int _baseCurrentHealthBoost = 20;
        public Bandage(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ПОВЯЗКА", "Менее грязная тряпка, из тех, что здесь обычно встречаются.", itemIdFactory.Id(), roomId, fromShop, _baseMaxHealthBoost, _baseCurrentHealthBoost) { }
    }
    public class RegenPotion : Heal
    {
        private const int _baseMaxHealthBoost = 0;
        private const int _baseCurrentHealthBoost = 60;
        public RegenPotion(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ЗЕЛЬЕ РЕГЕНЕРАЦИИ", "Пыльный бутылёк с субстанцией тёмного цвета.", itemIdFactory.Id(), roomId, fromShop, _baseMaxHealthBoost, _baseCurrentHealthBoost) { }
    }
    public class PowerPotion : Heal
    {
        private const int _baseMaxHealthBoost = 10;
        private const int _baseCurrentHealthBoost = 30;
        public PowerPotion(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ЗЕЛЬЕ СИЛЫ", "Не только восстанавливает силы, но и придаёт новых.", itemIdFactory.Id(), roomId, fromShop, _baseMaxHealthBoost, _baseCurrentHealthBoost) { }
    }
    public class RandomPotion : Heal
    {
        private const int _baseMaxHealthBoost = 25;
        private const int _baseCurrentHealthBoost = 50;
        public RandomPotion(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("НЕИЗВЕСТНОЕ ЗЕЛЬЕ", "Пробирка с жижей непонятного цвета.", itemIdFactory.Id(), roomId, fromShop, null, null) { }
        public override void Use(GameSession gameSession)
        {
            double maxHealthFloor = _baseMaxHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadFloor;
            double maxHealthCeiling = _baseMaxHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadCeiling;
            double currentHealthFloor = _baseCurrentHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadFloor;
            double currentHealthCeiling = _baseCurrentHealthBoost * GameBalance.ApplyGain(_roomId) * GameBalance.SpreadCeiling;
            if (_fromShop)
            {
                maxHealthFloor *= 1 / GameBalance.ShopMultiplier;
                maxHealthCeiling *= GameBalance.ShopMultiplier;
                currentHealthFloor *= 1 / GameBalance.ShopMultiplier;
                currentHealthCeiling *= GameBalance.ShopMultiplier;
            }
            MaxHealthBoost = _random.Next((int)maxHealthFloor, (int)currentHealthCeiling + 1);
            CurrentHealthBoost = _random.Next((int)currentHealthFloor, (int)currentHealthCeiling + 1);
            base.Use(gameSession);
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
        public abstract int Attack(GameSession gameSession);
    }
    public class Fists : Weapon
    {
        public static readonly Fists DefaultFists = new Fists();
        private const int _baseDamage = 10;
        private const int _selfHarmProbabilityDivider = 2;

        public Fists() : base("КУЛАКИ", "То, что есть (почти) у каждого. Базовое оружие самозащиты. Может быть больно.", null, null, _baseDamage, 0, false) { }
        public override int Attack(GameSession gameSession)
        {
            var (min, max) = GameBalance.ApplySpread(_baseDamage, gameSession.CurrentRoom!.Number);
            int damage = _random.Next(min, max + 1);
            if (_random.Next(_selfHarmProbabilityDivider) == 0)
                gameSession.CurrentHealth -= damage / 2;
            return damage;
        }
    }
    public abstract class Sword : Weapon
    {
        public Sword(string name, string description, IItemIdFactory itemIdFactory, int roomId, bool fromShop, int durability, int damage) : base(name, description, itemIdFactory.Id(), null, 0, roomId, fromShop)
        {
            Initialize(durability, damage);
        }
        protected void Initialize(int durability, int damage)
        {
            var (minDurability, maxDurability) = GameBalance.ApplySpread(durability, _roomId);
            Durability = _random.Next(minDurability, maxDurability + 1);
            var (minDamage, maxDamage) = GameBalance.ApplySpread(damage, _roomId);
            Damage = _random.Next(minDamage, maxDamage + 1);
            if (_fromShop)
            {
                Durability = GameBalance.ApplyShopMultiplier((int)Durability!);
                Damage = GameBalance.ApplyShopMultiplier(Damage);
            }
            CalculateCost();
        }
        public override int Attack(GameSession gameSession)
        {
            Durability--;
            CalculateCost();
            if (Durability <= 0) BreakDown(gameSession);
            return Damage;
        }
        public void BreakDown(GameSession gameSession)
        {
            gameSession.RemoveWeapon();
        }
        private void CalculateCost()
        {
            Cost = 1 + ((Durability * Damage) / 10);
        }
    }
    public class RustSword : Sword
    {
        private const int _baseDurability = 8;
        private const int _baseDamage = 15;
        public RustSword(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("РЖАВЫЙ МЕЧ", "Очень старый меч. Лучше, чем ничего.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamage) { }
    }
    public class IronSword : Sword
    {
        private const int _baseDurability = 18;
        private const int _baseDamage = 26;
        public IronSword(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНЫЙ МЕЧ", "Добротное оружие воина. На лезвии оттиск \"304\".Номер, наверное.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamage) { }
    }
    public class SilverSword : Sword
    {
        private const int _baseDurability = 13;
        private const int _baseDamage = 53;
        public SilverSword(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("СЕРЕБРЯНЫЙ МЕЧ", "Редкий меч из особого серебряного сплава. Эффективный, но менее прочный.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamage) { }
    }
    public class GlassSword : Sword
    {
        private const int _baseDurability = 1;
        private const int _baseDamage = 115;
        public GlassSword(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("СТЕКЛЯННЫЙ МЕЧ", "Скорее объект искусства, чем оружие. Очень хрупкий, но невероятно сильный.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamage) { }
    }
    public abstract class Wand : Weapon
    {
        public Wand(string name, string description, IItemIdFactory itemIdFactory, int roomId, bool fromShop, int damage) : base(name, description, itemIdFactory.Id(), null, 0, roomId, fromShop)
        {
            Initialize(damage);
        }
        protected void Initialize(int damage)
        {
            var (min, max) = GameBalance.ApplySpread(damage, _roomId);
            Damage = _random.Next(min, max + 1);
            if (_fromShop)
            {
                Damage = GameBalance.ApplyShopMultiplier(Damage);
            }
            Cost = Damage * 3;
        }
        public override int Attack(GameSession gameSession)
        {
            return Damage;
        }
    }
    public class MagicWand : Wand
    {
        private const int _baseDamage = 21;
        public MagicWand(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ВОЛШЕБНЫЙ ЖЕЗЛ", "Простое магическое оружие. Может использовать каждый.", itemIdFactory, roomId, fromShop, _baseDamage) { }
    }
    public class RandomWand : Wand
    {
        private const int _baseDamage = 41;
        public RandomWand(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ЖЕЗЛ СЛУЧАЙНОСТЕЙ", "Странное магическое оружие. Становится сильнее со временем.", itemIdFactory, roomId, fromShop, _baseDamage) { }
        public override int Attack(GameSession gameSession)
        {
            int damage = (int)(Damage * GameBalance.ApplyGain(_roomId));
            return _random.Next(damage + 1);
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
            Durability = _random.Next(
                (int)(durability * GameBalance.SpreadFloor),
                (int)(durability * GameBalance.SpreadCeiling + 1));
            var (min, max) = GameBalance.ApplySpread(damageBlock, _roomId);
            DamageBlock = _random.Next(min, max + 1);
            if (_fromShop)
            {
                Durability = GameBalance.ApplyShopMultiplier((int)Durability!);
                DamageBlock = GameBalance.ApplyShopMultiplier(DamageBlock);
            }
            CalculateCost();
        }
        public int Block(GameSession gameSession)
        {
            Durability--;
            CalculateCost();
            if (Durability <= 0) BreakDown(gameSession);
            return DamageBlock;
        }
        protected abstract void BreakDown(GameSession gameSession);
        private void CalculateCost()
        {
            Cost = 1 + ((Durability * DamageBlock) / 10);
        }
    }
    public abstract class Helm : Armor
    {
        public Helm(string name, string description, IItemIdFactory itemIdFactory, int roomId, bool fromShop, int durability, int damageBlock)
            : base(name, description, itemIdFactory.Id(), roomId, fromShop, durability, damageBlock) { }
        protected override void BreakDown(GameSession gameSession)
        {
            gameSession.RemoveHelm();
        }
    }
    public class WoodenBucket : Helm
    {
        private const int _baseDurability = 3;
        private const int _baseDamageBlock = 2;
        public WoodenBucket(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ДЕРЕВЯННОЕ ВЕДРО", "Старое дырявое ведро. Кто в своём уме наденет его на голову?", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamageBlock) { }
    }
    public class LeatherHelm : Helm
    {
        private const int _baseDurability = 12;
        private const int _baseDamageBlock = 4;
        public LeatherHelm(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("КОЖАННЫЙ ШЛЕМ", "Изысканный чёрный шлем мастера подземелия.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamageBlock) { }
    }
    public class IronHelm : Helm
    {
        private const int _baseDurability = 18;
        private const int _baseDamageBlock = 6;
        public IronHelm(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНЫЙ ШЛЕМ", "Крепкий шлем из качественного металла.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamageBlock) { }
    }
    public abstract class Chestplate : Armor
    {
        public Chestplate(string name, string description, IItemIdFactory itemIdFactory, int roomId, bool fromShop, int durability, int damageBlock)
            : base(name, description, itemIdFactory.Id(), roomId, fromShop, durability, damageBlock) { }
        protected override void BreakDown(GameSession gameSession)
        {
            gameSession.RemoveChestplate();
        }
    }
    public class LeatherVest : Chestplate
    {
        private const int _baseDurability = 24;
        private const int _baseDamageBlock = 8;
        public LeatherVest(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("КОЖАННАЯ КУРТКА", "Лёгкая куртка из плотной кожи.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamageBlock) { }

    }
    public class IronCuirass : Chestplate
    {
        private const int _baseDurability = 36;
        private const int _baseDamageBlock = 12;
        public IronCuirass(IItemIdFactory itemIdFactory, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНАЯ КИРАСА", "Тяжёлая и прочная. Имеет небольшой оттиск \"304\" на внутренней части.", itemIdFactory, roomId, fromShop, _baseDurability, _baseDamageBlock) { }
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
    #region ItemIdFactory
    public interface IItemIdFactory : IIdFactory { };
    public class ItemIdFactory : IItemIdFactory
    {
        private int ItemId = 0;
        public int Id()
        {
            return Interlocked.Increment(ref ItemId);
        }
        public void Reset()
        {
            ItemId = 0;
        }
    }
    #endregion
    #region ItemFactory
    public interface IItemFactory
    {
        public Item? CreateRoomItem(int roomId);
        public Item? CreateChestItem(int roomId);
        public Item? CreateShopItem(int roomId);
    }
    public class ItemFactory : IItemFactory
    {
        private readonly IItemIdFactory _itemIdFactory;
        private readonly IEnemyFactory _enemyFactory;
        private Random _random = Random.Shared;

        private const double _roomOtherPercent = 70;
        private const double _roomWeaponPercent = 10;
        private const double _roomArmorPercent = 5;
        private const double _roomHealPercent = 15;

        private const double _chestOtherPercent = 45;
        private const double _chestWeaponPercent = 20;
        private const double _chestArmorPercent = 10;
        private const double _chestHealPercent = 25;

        private const double _shopOtherPercent = 18;
        private const double _shopWeaponPercent = 28;
        private const double _shopArmorPercent = 27;
        private const double _shopHealPercent = 27;

        public ItemFactory(IItemIdFactory itemIdFactory, IEnemyFactory enemyFactory)
        {
            _itemIdFactory = itemIdFactory;
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
            int roll = _random.Next(totalWeight);
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
    double groupWeightPercent,
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
                int absoluteWeight = (int)(groupWeightPercent * (rawWeight / groupSum));

                if (absoluteWeight > 0)
                    options.Add((absoluteWeight, items[i].Creator));
            }
        }

        public Item? CreateRoomItem(int roomId)
        {
            //int rustSwordWeight = roomId < 100 ? (int)((-0.89 * roomId) + 89) : 0;
            //int ironSwordWeight = (int)((0.25 * roomId) + 5);
            //int silverSwordWeight = (int)(0.15 * roomId);
            //int glassSwordWeight = 1;

            //int magicWandWeight = (int)((0.25 * roomId) + 5);
            //int randomWandWeight = (int)(0.04 * roomId);

            //double weaponWieghts = rustSwordWeight + ironSwordWeight + silverSwordWeight + magicWandWeight + randomWandWeight;
            //double weaponMultiplier = (10 * 20 - glassSwordWeight) / weaponWieghts;

            //int woodenBucketWeight = roomId < 50 ? (int)((-1.7 * roomId) + 85) : 0;
            //int leatherHelm = (int)(0.35 * roomId + 10);
            //int ironHelm = (int)(0.2 * roomId);
            //int leatherVest = (int)(0.2 * roomId + 5);
            //int ironCuirass = (int)(0.1 * roomId);

            //double armorWeight = woodenBucketWeight + leatherHelm + ironHelm + leatherVest + ironCuirass;
            //double armorMultiplier = (5 * 20) / armorWeight;

            //int bandageWeight = roomId < 100 ? (int)(-0.34 * roomId + 67) : 33 * 20;
            //int regenWeight = (int)(0.22 * roomId + 22);
            //int powerWeight = (int)(0.11 * roomId + 10);
            //int randomWeight = (int)(0.01 * roomId + 1);

            //double healWeight = bandageWeight + regenWeight + powerWeight + randomWeight;
            //double healMultiplier = (15 * 20) / healWeight;

            //List<(int Weight, Func<Item?> Item)> roomItems = new()
            //{
            //    (30 * 20, () => null),
            //    (6 * 20,  () => new Key(_itemIdFactory,roomId)),
            //    (14 * 20,  () => new BagOfCoins(_itemIdFactory,roomId)),
            //    (20 * 20,  () => new Chest(_itemIdFactory,this,_enemyFactory,roomId)),
            //    //10, Weapon
            //    ((int)(weaponMultiplier * rustSwordWeight), () => new RustSword(_itemIdFactory,roomId,false)),
            //    ((int)(weaponMultiplier * ironSwordWeight), () => new IronSword(_itemIdFactory,roomId,false)),
            //    ((int)(weaponMultiplier * silverSwordWeight), () => new SilverSword(_itemIdFactory,roomId,false)),
            //    (glassSwordWeight, () => new GlassSword(_itemIdFactory,roomId,false)),
            //    ((int)(weaponMultiplier * magicWandWeight), () => new MagicWand(_itemIdFactory,roomId,false)),
            //    ((int)(weaponMultiplier * randomWandWeight), () => new RandomWand(_itemIdFactory,roomId,false)),
            //    //5, Armor
            //    ((int)(armorMultiplier * woodenBucketWeight), () => new WoodenBucket(_itemIdFactory,roomId,false)),
            //    ((int)(armorMultiplier * leatherHelm), () => new LeatherHelm(_itemIdFactory,roomId,false)),
            //    ((int)(armorMultiplier * ironHelm), () => new IronHelm(_itemIdFactory,roomId,false)),
            //    ((int)(armorMultiplier * leatherVest), () => new LeatherVest(_itemIdFactory,roomId,false)),
            //    ((int)(armorMultiplier * ironCuirass), () => new IronСuirass(_itemIdFactory,roomId,false)),
            //    //15, Heal
            //    ((int)(healMultiplier * bandageWeight), () => new Bandage(_itemIdFactory,roomId,false)),
            //    ((int)(healMultiplier * regenWeight), () => new RegenPotion(_itemIdFactory,roomId,false)),
            //    ((int)(healMultiplier * powerWeight), () => new PowerPotion(_itemIdFactory,roomId,false)),
            //    ((int)(healMultiplier * randomWeight), () => new RandomPotion(_itemIdFactory,roomId,false)),
            //};

            ////Выбор
            //int weightsSum = roomItems.Sum(x => x.Weight);
            //int roll = _random.Next(weightsSum);
            //int accumulated = 0;

            //foreach (var option in roomItems)
            //{
            //    if (roll < accumulated + option.Weight) return option.Item();
            //    accumulated += option.Weight;
            //}
            //return null;

            var options = new List<(int Weight, Func<Item?> Creator)>();

            // Группа "Прочее" (70%)
            AddWeightedGroup(options, roomId, _roomOtherPercent,
                // Относительные веса внутри группы (не в процентах)
                (_ => 30, () => null),
                (_ => 6, () => new Key(_itemIdFactory, roomId)),
                (_ => 14, () => new BagOfCoins(_itemIdFactory, roomId)),
                (_ => 20, () => new Chest(_itemIdFactory, this, _enemyFactory, roomId))
            );

            // Группа "Оружие" (10%)
            AddWeightedGroup(options, roomId, _roomWeaponPercent,
                (r => r < 100 ? -0.89 * r + 89 : 0, () => new RustSword(_itemIdFactory, roomId, false)),
                (r => 0.25 * r + 5, () => new IronSword(_itemIdFactory, roomId, false)),
                (r => 0.15 * r, () => new SilverSword(_itemIdFactory, roomId, false)),
                (_ => 1, () => new GlassSword(_itemIdFactory, roomId, false)),

                (r => 0.25 * r + 5, () => new MagicWand(_itemIdFactory, roomId, false)),
                (r => 0.04 * r, () => new RandomWand(_itemIdFactory, roomId, false))
            );

            // Группа "Броня" (5%)
            AddWeightedGroup(options, roomId, _roomArmorPercent,
                (r => r < 50 ? -1.7 * r + 85 : 0, () => new WoodenBucket(_itemIdFactory, roomId, false)),
                (r => 0.35 * r + 10, () => new LeatherHelm(_itemIdFactory, roomId, false)),
                (r => 0.2 * r, () => new IronHelm(_itemIdFactory, roomId, false)),
                (r => 0.2 * r + 5, () => new LeatherVest(_itemIdFactory, roomId, false)),
                (r => 0.1 * r, () => new IronCuirass(_itemIdFactory, roomId, false))
            );

            // Группа "Зелья" (15%)
            AddWeightedGroup(options, roomId, _roomHealPercent,
                (r => r < 100 ? -0.34 * r + 67 : 0, () => new Bandage(_itemIdFactory, roomId, false)),
                (r => 0.22 * r + 22, () => new RegenPotion(_itemIdFactory, roomId, false)),
                (r => 0.11 * r + 10, () => new PowerPotion(_itemIdFactory, roomId, false)),
                (r => 0.01 * r + 1, () => new RandomPotion(_itemIdFactory, roomId, false))
            );

            return SelectRandom(options);
        }
        public Item? CreateChestItem(int roomId)
        {
            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, _chestOtherPercent,
                (_ => 10, () => new Key(_itemIdFactory, roomId)),
                (_ => 30, () => new BagOfCoins(_itemIdFactory, roomId)),
                (_ => 5, () => new Map(_itemIdFactory))
            );
            AddWeightedGroup(options, roomId, _chestWeaponPercent,
                (r => r < 50 ? -0.8 * r + 40 : 0, () => new RustSword(_itemIdFactory, roomId, false)),
                (r => 0.25 * r + 15, () => new IronSword(_itemIdFactory, roomId, false)),
                (r => 0.1 * r + 5, () => new SilverSword(_itemIdFactory, roomId, false)),
                (_ => 5, () => new GlassSword(_itemIdFactory, roomId, false)),

                (r => 0.05 * r + 25, () => new MagicWand(_itemIdFactory, roomId, false)),
                (_ => 10, () => new RandomWand(_itemIdFactory, roomId, false))
            );
            AddWeightedGroup(options, roomId, _chestArmorPercent,
                (r => -0.15 * r + 55, () => new LeatherHelm(_itemIdFactory, roomId, false)),
                (r => 0.15 * r + 25, () => new IronHelm(_itemIdFactory, roomId, false)),
                (r => -0.04 * r + 14, () => new LeatherVest(_itemIdFactory, roomId, false)),
                (r => 0.04 * r + 6, () => new IronCuirass(_itemIdFactory, roomId, false))
            );
            AddWeightedGroup(options, roomId, _chestHealPercent,
                (_ => 60, () => new RegenPotion(_itemIdFactory, roomId, false)),
                (_ => 30, () => new PowerPotion(_itemIdFactory, roomId, false)),
                (_ => 10, () => new RandomPotion(_itemIdFactory, roomId, false))
            );
            return SelectRandom(options);
        }
        public Item? CreateShopItem(int roomId)
        {
            var options = new List<(int Weight, Func<Item?> Creator)>();

            AddWeightedGroup(options, roomId, _shopOtherPercent,
                (_ => 14, () => new Key(_itemIdFactory, roomId)),
                (_ => 4, () => new Map(_itemIdFactory))
            );
            AddWeightedGroup(options, roomId, _shopWeaponPercent,
                (r => r < 30 ? -r + 30 : 0, () => new RustSword(_itemIdFactory, roomId, true)),
                (r => 0.15 * r + 20, () => new IronSword(_itemIdFactory, roomId, true)),
                (r => 0.15 * r + 5, () => new SilverSword(_itemIdFactory, roomId, true)),

                (_ => 30, () => new MagicWand(_itemIdFactory, roomId, true)),
                (_ => 15, () => new RandomWand(_itemIdFactory, roomId, true))
            );
            AddWeightedGroup(options, roomId, _shopArmorPercent,
                (r => r < 30 ? -1.67 * r + 50 : 0, () => new WoodenBucket(_itemIdFactory, roomId, true)),
                (r => 0.025 * r + 27.5, () => new LeatherHelm(_itemIdFactory, roomId, true)),
                (r => 0.375 * r + 12.5, () => new IronHelm(_itemIdFactory, roomId, true)),
                (r => 0.01 * r + 7, () => new LeatherVest(_itemIdFactory, roomId, true)),
                (r => 0.09 * r + 3, () => new IronCuirass(_itemIdFactory, roomId, true))
            );
            AddWeightedGroup(options, roomId, _shopHealPercent,
                (r => r < 200 ? -0.15 * r + 30 : 0, () => new Bandage(_itemIdFactory, roomId, true)),
                (r => 0.1 * r + 40, () => new RegenPotion(_itemIdFactory, roomId, true)),
                (r => 0.05 * r + 20, () => new PowerPotion(_itemIdFactory, roomId, true)),
                (_ => 10, () => new RandomPotion(_itemIdFactory, roomId, true))
            );
            return SelectRandom(options);
        }
    }
    #endregion
    #endregion
    #region Enemy
    public interface IEnemyIdFactory : IIdFactory { };
    public class EnemyIdFactory : IEnemyIdFactory
    {
        private int _enemyId = 0;
        public int Id()
        {
            return Interlocked.Increment(ref _enemyId);
        }
        public void Reset()
        {
            _enemyId = 0;
        }
    }
    public abstract class Enemy : GameObject
    {
        public int Id { get; protected set; }
        public int Health { get; protected set; } = 0;
        public int Damage { get; protected set; } = 0;
        public int DamageBlock { get; protected set; } = 0;

        private readonly int _roomId;

        protected Random _random = Random.Shared;

        public Enemy(string name, string description, int roomId, IEnemyIdFactory enemyIdFactory, int health, int damage, int damageBlock)
        {
            _roomId = roomId;

            Name = name;
            Description = description;
            Id = enemyIdFactory!.Id();
            Initialize(health, damage, damageBlock);
        }
        public virtual void Initialize(int health, int damage, int damageBlock)
        {
            var (minHealth, maxHealth) = GameBalance.ApplySpread(health, _roomId);
            Health = _random.Next(minHealth, maxHealth + 1);

            var (minDamage, maxDamage) = GameBalance.ApplySpread(damage, _roomId);
            Damage = _random.Next(minDamage, maxDamage + 1);

            var (minDamageBlock, maxDamageBlock) = GameBalance.ApplySpread(damageBlock, _roomId);
            DamageBlock = _random.Next(minDamageBlock, maxDamageBlock + 1);
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
        private const int _baseHealth = 20;
        private const int _baseDamage = 10;
        private const int _baseDamageBlock = 2;
        public Skeletor(int roomId, IEnemyIdFactory enemyIdFactory)
            : base("СКЕЛЕТОР", "Чей-то скелет, наделёный возможностью двигаться.", roomId, enemyIdFactory, _baseHealth, _baseDamage, _baseDamageBlock) { }
    }
    public class SkeletorArcher : Enemy
    {
        private const int _baseHealth = 10;
        private const int _baseDamage = 20;
        private const int _baseDamageBlock = 1;
        public SkeletorArcher(int roomId, IEnemyIdFactory enemyIdFactory)
            : base("СКЕЛЕТОР-ЛУЧНИК", "Из тех, кто при жизни умел обращаться с луком.", roomId, enemyIdFactory, _baseHealth, _baseDamage, _baseDamageBlock) { }
    }
    public class Deadman : Enemy
    {
        private const int _baseHealth = 50;
        private const int _baseDamage = 5;
        private const int _baseDamageBlock = 3;
        public Deadman(int roomId, IEnemyIdFactory enemyIdFactory)
            : base("МЕРТВЯК", "Мёртвое полуразложившееся тело. Источник жуткого смрада.", roomId, enemyIdFactory, _baseHealth, _baseDamage, _baseDamageBlock) { }
    }
    public class Ghost : Enemy
    {
        private const int _baseHealth = 15;
        private const int _baseDamage = 15;
        private const int _baseDamageBlock = 0;
        public Ghost(int roomId, IEnemyIdFactory enemyIdFactory)
            : base("ПРИЗРАК", "Злой полуматериальный дух. Попробуй попади.", roomId, enemyIdFactory, _baseHealth, _baseDamage, _baseDamageBlock) { }
        public override int GetDamage(int damage, Room? room = null)
        {
            if (Random.Shared.Next(2) == 0) Health -= damage;
            return Health;
        }
    }
    public class Lich : Enemy
    {
        private const int _baseHealth = 40;
        private const int _baseDamage = 30;
        private const int _baseDamageBlock = 8;
        public Lich(int roomId, IEnemyIdFactory enemyIdFactory)
            : base("ЛИЧ", "Тебя ждёт вечный параЛИЧ. Ха-ха.", roomId, enemyIdFactory, _baseHealth, _baseDamage, _baseDamageBlock) { }
    }
    public class Mimic : Enemy
    {
        private const int _baseHealth = 15;
        private const int _baseDamage = 10;
        private const int _baseDamageBlock = 5;
        public Mimic(int roomId, IEnemyIdFactory enemyIdFactory)
            : base("МИМИК", "Подлый монстр, изменяющий свой облик для охоты на неосторожных попаданцев.", roomId, enemyIdFactory, _baseHealth, _baseDamage, _baseDamageBlock) { }
    }

    public interface IEnemyFactory
    {
        Skeletor CreateSkeletor(int roomId);
        SkeletorArcher CreateSkeletorArcher(int roomId);
        Deadman CreateDeadman(int roomId);
        Ghost CreateGhost(int roomId);
        Lich CreateLich(int roomId);
        Mimic CreateMimic(int roomId);
    }
    public class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyIdFactory _enemyIdFactory;
        public EnemyFactory(IEnemyIdFactory enemyIdFactory) => _enemyIdFactory = enemyIdFactory;
        public Skeletor CreateSkeletor(int roomId) => new Skeletor(roomId, _enemyIdFactory);
        public SkeletorArcher CreateSkeletorArcher(int roomId) => new SkeletorArcher(roomId, _enemyIdFactory);
        public Deadman CreateDeadman(int roomId) => new Deadman(roomId, _enemyIdFactory);
        public Ghost CreateGhost(int roomId) => new Ghost(roomId, _enemyIdFactory);
        public Lich CreateLich(int roomId) => new Lich(roomId, _enemyIdFactory);
        public Mimic CreateMimic(int roomId) => new Mimic(roomId, _enemyIdFactory);
    }
    #endregion 
}