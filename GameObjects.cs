using System;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;

namespace TextGame
{
    public class GameObject
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
    #region Room
    public enum RoomType
    {
        EndRoom, //1:100

        EmptyRoom, //25:100
        SmallRoom, //44:100
        BigRoom, //25:100

        Shop, //5:100
    }
    public class Room : GameObject
    {
        private readonly IEnemyFactory? EnemyFactory;
        public int Number { get; init; }
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public bool IsDiscovered { get; set; } = false;

        public Room(string name, string description, int number)
        {
            Name = name;
            Description = description;
            Number = number;
            EnemyFactory = null;
        }
        public Room(string name, string description, int number, IEnemyFactory enemyFactory)
        {
            Name = name;
            Description = description;
            Number = number;
            EnemyFactory = enemyFactory;
        }
        public void CreateEnemy()
        {
            if (EnemyFactory == null) return;

            Enemy? enemy = EnemyFactory.CreateEnemy(Number);
            if (enemy != null) Enemies.Add(enemy);
        }
    }
    public class StartRoom : Room
    {
        public StartRoom() : base("СТАРТОВАЯ КОМАНТА", "В потолке дыра, через которую вы сюда провалились.", 0)
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
            CreateEnemy();
        }
    }
    public class SmallRoom : Room
    {
        public SmallRoom(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory) : base("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", roomNumberFactory.GetRoomNumber(), enemyFactory)
        {
            Item? item = itemFactory.CreateRoomItem(Number);
            if (item != null) Items.Add(item);
            CreateEnemy();
        }
    }
    public class BigRoom : Room
    {
        public BigRoom(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory) : base("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", roomNumberFactory.GetRoomNumber(), enemyFactory)
        {
            const int itemsAmount = 3;
            for (int i = 0; i < itemsAmount; i++)
            {
                Item? item = itemFactory.CreateRoomItem(Number);
                if (item != null) Items.Add(item);
            }
            CreateEnemy();
        }
    }
    public class Shop : Room
    {
        //private readonly IRoomItemFactory RoomItemFactory;

        public Shop(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory) : base("МАГАЗИН", "Здесь мутный торгаш продаёт своё добро.", roomNumberFactory.GetRoomNumber())
        {
            //RoomItemFactory = roomItemFactory;
            //DO Настроить магазин
            //for (int i = 0; i < 3; i++)
            //{
            //    Item? item = roomItemFactory.CreateItem();
            //    if (item != null) Items.Add(item);
            //}
        }
    }
    public interface IRoomNumberFactory
    {
        int GetRoomNumber();
        void Reset();
    }
    public class RoomNumberFactory : IRoomNumberFactory
    {
        private int Number = 0;
        public int GetRoomNumber()
        {
            Interlocked.Increment(ref Number);
            return Number;
        }
        public void Reset() => Number = 0;
    }

    public interface IRoomFactory
    {
        Room CreateRoom();
    }

    public class RoomFactory : IRoomFactory
    {
        private readonly IRoomNumberFactory RoomNumberFactory;
        private readonly IItemFactory ItemFactory;
        private readonly IEnemyFactory EnemyFactory;
        private readonly Random random = Random.Shared;

        public RoomFactory(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory, IEnemyFactory enemyFactory)
        {
            RoomNumberFactory = roomNumberFactory;
            ItemFactory = itemFactory;
            EnemyFactory = enemyFactory;
        }

        public Room CreateRoom()
        {
            const int EndRoomMin = 0;
            const int EndRoomMax = 0;
            const int SmallRoomMin = 26;
            const int SmallRoomMax = 69;
            const int BigRoomMin = 70;
            const int BigRoomMax = 94;
            //const int ShopMin = 95;
            //const int ShopMax = 99;


            var roomTypeNumber = random.Next(100);
            RoomType roomType = roomTypeNumber switch
            {
                >= EndRoomMin and <= EndRoomMax => RoomType.EndRoom,
                >= SmallRoomMin and <= SmallRoomMax => RoomType.SmallRoom,
                >= BigRoomMin and <= BigRoomMax => RoomType.BigRoom,
                //>= ShopMin and <= ShopMax => RoomType.Shop,

                _ => RoomType.EmptyRoom
            };

            return roomType switch
            {
                RoomType.SmallRoom => new SmallRoom(RoomNumberFactory, ItemFactory, EnemyFactory),
                RoomType.BigRoom => new BigRoom(RoomNumberFactory, ItemFactory, EnemyFactory),
                RoomType.EndRoom => new EndRoom(RoomNumberFactory),
                RoomType.Shop => new Shop(RoomNumberFactory, ItemFactory),
                _ => new EmptyRoom(RoomNumberFactory, EnemyFactory),
            };
        }
    }
    #endregion
    #region Item
    public class Item : GameObject
    {
        public int? Id { get; init; }
        public bool IsCarryable { get; init; }
        //public int Cost { get; init; }
        public Item(string? name, string? description, int? id, bool isCarryable)
        {
            Name = name;
            Description = description;
            Id = id;
            IsCarryable = isCarryable;
        }
    }
    //public enum ItemType
    //{
    //    //None,
    //    Key,
    //    Coin,
    //    Chest,
    //    Map,
    //}
    #region Key
    public class Key : Item
    {
        public Key(IItemIdFactory itemIdFactory) : base("КЛЮЧ", "Что-то открывает.", itemIdFactory!.Id(), true) { }
    }
    #endregion
    #region Coin
    public class Coin : Item
    {
        public Coin(IItemIdFactory itemIdFactory) : base("МОНЕТА", "Блестит. Она явно ценная.", itemIdFactory!.Id(), true) { }
    }
    #endregion
    #region Chest
    public class Chest : Item
    {
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; } = true;
        public bool IsMimic { get; private set; }
        public List<Item> Items { get; set; }

        private readonly int MinChestItemsAmount = 1;
        private readonly int MaxChestItemsAmount = 3;

        private readonly int MimicProbabilityDivider = 2; // 1/2
        private readonly int LockedProbabilityDivider = 2; // 1/2


        public Chest(IItemIdFactory itemIdFactory, IItemFactory itemFactory, int roomId, bool isShop) : base("СУНДУК", "Хранит предметы. Может оказаться мимиком.", itemIdFactory!.Id(), false)
        {
            var random = new Random();
            IsLocked = random.Next(LockedProbabilityDivider) == 0;
            IsMimic = random.Next(MimicProbabilityDivider) == 0;
            Items = new List<Item>();
            var itemsInChest = random.Next(MinChestItemsAmount, MaxChestItemsAmount + 1);
            for (int i = 0; i < itemsInChest; i++)
            {
                Item? item = itemFactory.CreateChestItem(roomId, isShop);
                if (item != null) Items.Add(item);
            }
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public List<Item> Search() => Items;
        public void KillMimic()
        {
            IsMimic = false;
            Name = "МЁРТВЫЙ МИМИК";
            Description = "Мёртвый сундук с руками и зубами. Интересно, что у него внутри.";
            IsLocked = false;
            IsClosed = false;
        }
    }
    #endregion
    #region Map
    public class Map : Item
    {
        public Map(IItemIdFactory itemIdFactory) : base("КАРТА", "Содержит знания о строении подземелья.", itemIdFactory!.Id(), true) { }
    }
    #endregion
    #region Heal
    public abstract class Heal : Item
    {
        public int? MaxHealthBoost { get; protected set; } = 0;
        public int? CurrentHealthBoost { get; protected set; } = 0;
        protected double Multiplicator = 1;
        private readonly int MultiplicatorDivider = 20;
        public Heal(string name, string description, int id, int roomId) : base(name, description, id, true)
        {
            Multiplicator = 1 + (roomId / MultiplicatorDivider);
        }
        protected virtual void Initialize(int? maxHealthBoost, int? currentHealthBoost)
        {
            if (maxHealthBoost == null) MaxHealthBoost = null;
            else MaxHealthBoost = (int)Math.Round((int)maxHealthBoost * Multiplicator);

            if (currentHealthBoost == null) CurrentHealthBoost = null;
            else CurrentHealthBoost = (int)Math.Round((int)currentHealthBoost * Multiplicator);
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
        public Bandage(IItemIdFactory itemIdFactory, int roomId) : base("ПОВЯЗКА", "Менее грязная тряпка, из тех, что здесь обычно встречаются.", itemIdFactory.Id(), roomId)
        {
            Initialize(0, 2);
        }

        public override void Use(GameSession gameSession)
        {
            if ((gameSession.CurrentHealth + CurrentHealthBoost) >= gameSession.MaxHealth)
                gameSession.CurrentHealth = gameSession.MaxHealth;
            else
                gameSession.CurrentHealth += (int)CurrentHealthBoost!;
        }
    }
    public class RegenPotion : Heal
    {
        public RegenPotion(IItemIdFactory itemIdFactory, int roomId) : base("ЗЕЛЬЕ РЕГЕНЕРАЦИИ", "Пыльный бутылёк с субстанцией тёмного цвета.", itemIdFactory.Id(), roomId)
        {
            Initialize(0, 6);
        }
    }
    public class PowerPotion : Heal
    {
        public PowerPotion(IItemIdFactory itemIdFactory, int roomId) : base("ЗЕЛЬЕ СИЛЫ", "Не только восстанавливает силы, но и придаёт новых.", itemIdFactory.Id(), roomId)
        {
            Initialize(2, 4);
        }
    }
    public class RandomPotion : Heal
    {
        private Random Random = new Random();
        public RandomPotion(IItemIdFactory itemIdFactory, int roomId) : base("НЕИЗВЕСТНОЕ ЗЕЛЬЕ", "Пробирка с жижей непонятного цвета.", itemIdFactory.Id(), roomId)
        {
            Initialize(null, null);
        }
        public override void Use(GameSession gameSession) //дублирование кода
        {
            int maxHealthBoost = (int)Math.Round((Random.Next(11) - 5) * Multiplicator);
            int currentHealthBoost = (int)Math.Round((Random.Next(11) - 5) * Multiplicator);

            if (maxHealthBoost != 0)
            {
                gameSession.MaxHealth += maxHealthBoost!;
                gameSession.CurrentHealth += maxHealthBoost!;
                if (gameSession.MaxHealth <= 0) gameSession.MaxHealth = 1;
            }
            if (currentHealthBoost != 0)
            {
                if ((gameSession.CurrentHealth + currentHealthBoost!) >= gameSession.MaxHealth)
                    gameSession.CurrentHealth = gameSession.MaxHealth;
                else
                    gameSession.CurrentHealth += currentHealthBoost!;
            }
        }
    }
    #endregion
    #region Equipment
    public abstract class Equipment : Item
    {
        public int? Durability;
        protected double Multiplicator;

        private const double FromShopMultiplicator = 1.2;
        private const int MultiplicatorDivider = 100;
        public Equipment(string? name, string? description, int? id, int? durability, int roomId, bool fromShop) : base(name, description, id, true)
        {
            Multiplicator = fromShop ? (1 + (roomId / MultiplicatorDivider)) * FromShopMultiplicator : (1 + (roomId / MultiplicatorDivider));
            Durability = durability;
        }
    }
    #region Weapon
    public abstract class Weapon : Equipment
    {
        public int? Damage;
        public Weapon(string? name, string? description, int? id, int? durability, int? damage, int roomId, bool fromShop)
            : base(name, description, id, durability, roomId, fromShop)
        {
            Damage = damage;
        }
        public abstract int Attack(GameSession gameSession);
    }
    #region Fists
    public class Fists : Weapon
    {
        public static readonly Fists DefaultFists = new Fists();
        public static readonly int SelfHarmProbabilityDivider = 2;
        public static readonly int FistsDamageIncreaseDivider = 20;

        public Fists() : base("КУЛАКИ", "То, что есть (почти) у каждого. Базовое оружие самозащиты. Может быть больно.", null, null, 1, 1, false) { }
        public override int Attack(GameSession gameSession)
        {
            Random random = new Random();
            double Multiplicator = gameSession.CurrentRoom!.Number / FistsDamageIncreaseDivider;
            if (random.Next((int)Math.Round(SelfHarmProbabilityDivider + Multiplicator)) == 0) gameSession.CurrentHealth--;
            int damage = (int)Math.Round((int)Damage! + Multiplicator);
            return damage;
        }
    }
    #endregion
    #region Swords
    enum SwordType
    {
        Rust,
        Iron,
        Silver,
        Glass,
    }
    public class Sword : Weapon
    {
        private SwordType SwordType;

        private const int RustSwordMax = 70;
        private const int IronSwordMax = 95;
        private const int SilverSwordMax = 99;
        public Sword(IItemIdFactory itemIdFactory, int roomId, bool fromShop) : base(null, null, itemIdFactory.Id(), null, null, roomId, fromShop)
        {
            Random random = new Random();
            int swordTypeNumber = random.Next(100);
            SwordType = swordTypeNumber switch
            {
                >= 0 and < RustSwordMax => SwordType.Rust,
                >= RustSwordMax and < IronSwordMax => SwordType.Iron,
                >= IronSwordMax and < SilverSwordMax => SwordType.Silver,
                >= SilverSwordMax and < 100 => SwordType.Glass,

                _ => SwordType.Rust,
            };
            switch (SwordType)
            {
                case SwordType.Rust:
                    Initialize("РЖАВЫЙ МЕЧ", "Очень старый меч. Лучше, чем ничего.", random.Next(1, 11), random.Next(3, 8));
                    break;
                case SwordType.Iron:
                    Initialize("ЖЕЛЕЗНЫЙ МЕЧ", "Добротное оружие воина.", random.Next(1, 101), random.Next(8, 17));
                    break;
                case SwordType.Silver:
                    Initialize("СЕРЕБРЯНЫЙ МЕЧ", "Редкий меч из особого серебряного сплава. Эффективный, но менее прочный.", random.Next(10, 51), random.Next(25, 31));
                    break;
                case SwordType.Glass:
                    Initialize("СТЕКЛЯННЫЙ МЕЧ", "Скорее объект искусства, чем оружие. Очень хрупкий, но невероятно сильный.", 1, 100);
                    break;
            }
        }
        private void Initialize(string name, string description, int durability, int damage)
        {
            Name = name;
            Description = description;
            Durability = durability;
            Damage = (int)Math.Round(damage * Multiplicator);
        }
        public override int Attack(GameSession gameSession)
        {
            Durability--;
            if (Durability <= 0) BreakDown(gameSession);
            return (int)Damage!;
        }
        public void BreakDown(GameSession gameSession)
        {
            gameSession.RemoveWeapon();
        }
    }
    #endregion
    #region Wands
    enum WandType
    {
        Magic,
        Random,
    }
    public class Wand : Weapon
    {
        private WandType WandType;

        private const int RandomWandMaxDamage = 40;

        private const int MagicWandMax = 90;
        public Wand(IItemIdFactory itemIdFactory, int roomId, bool fromShop) : base(null, null, itemIdFactory.Id(), null, null, roomId, fromShop)
        {
            Random random = new Random();
            int wandTypeNumber = random.Next(100);
            WandType = wandTypeNumber switch
            {
                >= 0 and < MagicWandMax => WandType.Magic,
                >= MagicWandMax and < 100 => WandType.Random,

                _ => WandType.Magic,
            };
            switch (WandType)
            {
                case WandType.Magic:
                    Initialize("ВОЛШЕБНЫЙ ЖЕЗЛ", "Простое магическое оружие. Может использовать каждый.", random.Next(7, 14));
                    break;
                case WandType.Random:
                    Initialize("ЖЕЗЛ СЛУЧАЙНОСТЕЙ", "Странное магическое оружие. Становится сильнее со временем.", RandomWandMaxDamage);
                    break;
            }
        }
        private void Initialize(string name, string description, int damage)
        {
            Name = name;
            Description = description;
            if (WandType == WandType.Random) Damage = RandomWandMaxDamage;
            else Damage = (int)Math.Round(damage * Multiplicator);
        }
        public override int Attack(GameSession gameSession)
        {
            if (WandType == WandType.Random)
            {
                Random random = new Random();
                int damage = (int)Math.Round((int)Damage! * Multiplicator);
                return random.Next(damage + 1);
            }
            return (int)Damage!;
        }
    }
    #endregion
    #endregion
    #region Armor
    public abstract class Armor : Equipment
    {
        public int DamageBlock;
        public Armor(string? name, string? description, int id, int? durability, int? damageBlock, int roomId, bool fromShop) : base(name, description, id, durability, roomId, fromShop) { }
        public abstract int Block(GameSession gameSession);
    }
    #region Helm
    enum HelmType
    {
        WoodenBucket,
        Leather,
        Iron,
    }
    public class Helm : Armor
    {
        private HelmType HelmType;

        private const int WoodenBucketMax = 70;
        private const int LeatherMax = 80;

        public Helm(IItemIdFactory itemIdFactory, int roomId, bool fromShop) : base(null, null, itemIdFactory.Id(), null, null, roomId, fromShop)
        {
            Random random = new Random();
            int helmTypeNumber = random.Next(100);
            HelmType = helmTypeNumber switch
            {
                >= 0 and < WoodenBucketMax => HelmType.WoodenBucket,
                >= WoodenBucketMax and < LeatherMax => HelmType.Leather,
                >= LeatherMax and < 100 => HelmType.Iron,

                _ => HelmType.WoodenBucket,
            };
            switch (HelmType)
            {
                case HelmType.WoodenBucket:
                    Initialize("ДЕРЕВЯННОЕ ВЕДРО", "Старое дырявое ведро. Кто в своём уме наденет его на голову?", random.Next(2, 6), random.Next(1, 3));
                    break;
                case HelmType.Leather:
                    Initialize("КОЖАННЫЙ ШЛЕМ", "Изысканный чёрный шлем мастера подземелия.", random.Next(7, 15), random.Next(3, 7));
                    break;
                case HelmType.Iron:
                    Initialize("ЖЕЛЕЗНЫЙ ШЛЕМ", "Крепкий шлем из качественного металла.", random.Next(16, 31), random.Next(8, 12));
                    break;
            }
        }
        private void Initialize(string name, string description, int durability, int damageBlock)
        {
            Name = name;
            Description = description;
            Durability = durability;
            DamageBlock = (int)Math.Round(damageBlock * Multiplicator);
        }
        public override int Block(GameSession gameSession)
        {
            Durability--;
            if (Durability <= 0) BreakDown(gameSession);
            return DamageBlock;
        }
        public void BreakDown(GameSession gameSession)
        {
            gameSession.RemoveHelm();
        }
    }
    #endregion
    #region Chestplate
    enum ChestplateType
    {
        Leather,
        Iron,
    }
    public class Chestplate : Armor
    {
        private ChestplateType ChestplateType;

        private const int LeatherMax = 80;
        public Chestplate(IItemIdFactory itemIdFactory, int roomId, bool fromShop) : base(null, null, itemIdFactory.Id(), null, null, roomId, fromShop)
        {
            Random random = new Random();
            int chestplateTypeNumber = random.Next(100);
            ChestplateType = chestplateTypeNumber switch
            {
                >= 0 and < LeatherMax => ChestplateType.Leather,
                >= LeatherMax and < 100 => ChestplateType.Iron,

                _ => ChestplateType.Leather,
            };
            switch (ChestplateType)
            {
                case ChestplateType.Leather:
                    Initialize("КОЖАННАЯ КУРТКА", "Лёгкая куртка из плотной кожи.", random.Next(5, 16), random.Next(6, 14));
                    break;
                case ChestplateType.Iron:
                    Initialize("ЖЕЛЕЗНАЯ КИРАСА", "Тяжёлая и прочная.", random.Next(20, 51), random.Next(16, 24));
                    break;
            }
        }
        private void Initialize(string name, string description, int durability, int damageBlock)
        {
            Name = name;
            Description = description;
            Durability = durability;
            DamageBlock = (int)Math.Round(damageBlock * Multiplicator);
        }
        public override int Block(GameSession gameSession)
        {
            Durability--;
            if (Durability <= 0) BreakDown(gameSession);
            return DamageBlock;
        }
        public void BreakDown(GameSession gameSession)
        {
            gameSession.RemoveChestplate();
        }
    }
    #endregion
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
    enum Items
    {
        None,
        Key,
        Coin,
        Chest,
        Map,

        Bandage,
        RegenPotion,
        PowerPotion,
        RandomPotion,

        Sword,
        Wand,

        Helm,
        Chestplate,
    }
    public interface IItemFactory
    {
        public Item? CreateRoomItem(int roomId, bool isShop = false);
        public Item? CreateChestItem(int roomId, bool isShop = false);
        //shop
    }
    public class ItemFactory : IItemFactory
    {
        private readonly IItemIdFactory ItemIdFactory;
        private Random Random = Random.Shared;

        #region RoomItemsConts
        //62/100
        private const int RoomNoneMax = 23; // 23/100
        private const int RoomKeyMax = 36; // 13/100
        private const int RoomCoinMax = 49; // 13/100
        private const int RoomChestMax = 62; // 13/100
        //8/100
        private const int RoomBondageMax = 70;
        //13/100
        private const int RoomSwordMax = 80; // 10/100
        private const int RoomWandMax = 85; // 5/100
        //13/100
        private const int RoomHelmMax = 95; // 10/100
        //private const int RoomChestplateMax = 100; // 5/100
        #endregion
        #region ChestItemsConts
        // 50/100
        private const int ChestNoneMax = 10; // 10/100
        private const int ChestKeyMax = 20; // 10/100
        private const int ChestCoinMax = 45; // 25/100
        private const int ChestMapMax = 50; // 5/100
        // 15/100
        private const int ChestRegenPotionMax = 58; // 8/100
        private const int ChestPowerPotionMax = 63; // 5/100
        private const int ChestRandomPotionMax = 65; // 2/100
        // 20/100
        private const int ChestSwordMax = 72; // 7/100
        private const int ChestWandMax = 85; // 13/100
        // 15/100
        private const int ChestHelmMax = 93; // 8/100
        //private const int ChestChestplateMax = 100; // 7/100
        #endregion
        public ItemFactory(IItemIdFactory itemIdFactory)
        {
            ItemIdFactory = itemIdFactory;
        }
        public Item? CreateRoomItem(int roomId, bool isShop = false)
        {
            var itemNumber = Random.Next(100);

            Items roomItem = itemNumber switch
            {
                >= 0 and <= RoomNoneMax => Items.None,
                >= RoomNoneMax and < RoomKeyMax => Items.Key,
                >= RoomKeyMax and < RoomCoinMax => Items.Coin,
                >= RoomCoinMax and < RoomChestMax => Items.Chest,
                >= RoomChestMax and < RoomBondageMax => Items.Bandage,

                >= RoomBondageMax and < RoomSwordMax => Items.Sword,
                >= RoomSwordMax and < RoomWandMax => Items.Wand,

                >= RoomWandMax and < RoomHelmMax => Items.Helm,
                >= RoomHelmMax and < 100 => Items.Chestplate,

                _ => Items.None,
            };

            return roomItem switch
            {
                Items.Key => new Key(ItemIdFactory),
                Items.Coin => new Coin(ItemIdFactory),
                Items.Chest => new Chest(ItemIdFactory, this, roomId, isShop),
                Items.Bandage => new Bandage(ItemIdFactory, roomId),

                Items.Sword => new Sword(ItemIdFactory, roomId, isShop),
                Items.Wand => new Wand(ItemIdFactory, roomId, isShop),

                Items.Helm => new Helm(ItemIdFactory, roomId, isShop),
                Items.Chestplate => new Chestplate(ItemIdFactory, roomId, isShop),

                _ => null,
            };
        }
        public Item? CreateChestItem(int roomId, bool isShop = false)
        {
            var itemNumber = Random.Next(100);

            Items chestItem = itemNumber switch
            {
                >= 0 and <= ChestNoneMax => Items.None,
                >= ChestNoneMax and < ChestKeyMax => Items.Key,
                >= ChestKeyMax and < ChestCoinMax => Items.Coin,
                >= ChestCoinMax and < ChestMapMax => Items.Map,

                >= ChestMapMax and < ChestRegenPotionMax => Items.RegenPotion,
                >= ChestRegenPotionMax and < ChestPowerPotionMax => Items.PowerPotion,
                >= ChestPowerPotionMax and < ChestRandomPotionMax => Items.RandomPotion,

                >= ChestRandomPotionMax and < ChestSwordMax => Items.Sword,
                >= ChestSwordMax and < ChestWandMax => Items.Wand,

                >= ChestWandMax and < ChestHelmMax => Items.Helm,
                >= ChestHelmMax and < 100 => Items.Chestplate,

                _ => Items.None,
            };
            return chestItem switch
            {
                Items.Key => new Key(ItemIdFactory),
                Items.Coin => new Coin(ItemIdFactory),
                Items.Map => new Map(ItemIdFactory),

                Items.Sword => new Sword(ItemIdFactory, roomId, isShop),
                Items.Wand => new Wand(ItemIdFactory, roomId, isShop),

                Items.Helm => new Helm(ItemIdFactory, roomId, isShop),
                Items.Chestplate => new Chestplate(ItemIdFactory, roomId, isShop),


                _ => null,
            };
        }
    }
    #endregion
    #endregion
    #region Enemy
    public interface IEnemyIdFactory : IIdFactory { };
    public class EnemyIdFactory : IEnemyIdFactory
    {
        private int EnemyId = 0;
        public int Id()
        {
            return Interlocked.Increment(ref EnemyId);
        }
        public void Reset()
        {
            EnemyId = 0;
        }
    }
    public abstract class Enemy : GameObject
    {
        public int Id { get; protected set; }
        public int Health { get; protected set; } = 0;
        public int Damage { get; protected set; } = 0;
        public int DamageBlock { get; protected set; } = 0;

        private double Multiplicator;
        private const int MultiplicatorDivider = 50;

        public Enemy(string name, string description, int roomId, IEnemyIdFactory enemyIdFactory)
        {
            Name = name;
            Description = description;
            Id = enemyIdFactory!.Id();
            Multiplicator = 1 + (roomId / MultiplicatorDivider);
        }
        public virtual void Initialize(int health, int damage, int damageBlock)
        {
            Health = (int)Math.Round(health * Multiplicator);
            Damage = (int)Math.Round(damage * Multiplicator);
            DamageBlock = (int)Math.Round(damageBlock * Multiplicator);
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
        public Skeletor(int roomId, IEnemyIdFactory enemyIdFactory) : base("СКЕЛЕТОР", "Чей-то скелет, наделёный возможностью двигаться.", roomId, enemyIdFactory)
        {
            var random = Random.Shared;
            int health = random.Next(2, 5);
            int damage = random.Next(1, 3);
            Initialize(health, damage, 0);
        }
    }
    public class SkeletorArcher : Enemy
    {
        public SkeletorArcher(int roomId, IEnemyIdFactory enemyIdFactory) : base("СКЕЛЕТОР-ЛУЧНИК", "Из тех, кто при жизни умел обращаться с луком.", roomId, enemyIdFactory)
        {
            var random = Random.Shared;
            int health = random.Next(1, 3);
            int damage = random.Next(3, 5);
            Initialize(health, damage, 0);
        }
    }
    public class Deadman : Enemy
    {
        public Deadman(int roomId, IEnemyIdFactory enemyIdFactory) : base("МЕРТВЯК", "Мёртвое полуразложившееся тело. Источник жуткого смрада.", roomId, enemyIdFactory)
        {
            var random = Random.Shared;
            int health = random.Next(4, 7);
            int damage = random.Next(1, 3);
            int damageBlock = random.Next(1, 3);
            Initialize(health, damage, damageBlock);
        }
    }
    public class Ghost : Enemy
    {
        public Ghost(int roomId, IEnemyIdFactory enemyIdFactory) : base("ПРИЗРАК", "Злой полуматериальный дух. Попробуй попади.", roomId, enemyIdFactory)
        {
            var random = Random.Shared;
            int health = random.Next(3, 5);
            int damage = random.Next(2, 5);
            Initialize(health, damage, 0);
        }
        public override int GetDamage(int damage, Room? room = null)
        {
            if (Random.Shared.Next(2) == 0) Health -= damage;
            return Health;
        }
    }
    public class Lich : Enemy
    {
        public Lich(int roomId, IEnemyIdFactory enemyIdFactory) : base("ЛИЧ", "Тебя ждёт вечный параЛИЧ. Ха-ха.", roomId, enemyIdFactory)
        {
            var random = Random.Shared;
            int health = random.Next(5, 13);
            int damage = random.Next(4, 8);
            int damageBlock = random.Next(4, 7);
            Initialize(health, damage, damageBlock);
        }
    }
    public class Mimic : Enemy
    {
        private Chest Chest;
        public Mimic(int roomId, Chest chest, IEnemyIdFactory enemyIdFactory) : base("МИМИК", "Подлый монстр, изменяющий свой облик для охоты на неосторожных попаданцев.", roomId, enemyIdFactory)
        {
            Chest = chest;
            var random = Random.Shared;
            int health = random.Next(3, 6);
            int damage = random.Next(3, 5);
            int damageBlock = random.Next(2, 4);
            Initialize(health, damage, damageBlock);
        }
        public override int GetDamage(int damage, Room? room = null)
        {
            if (damage > DamageBlock) Health -= damage - DamageBlock;
            if (Health <= 0)
            {
                Chest.KillMimic();
                room!.Items.Add(Chest);
            }
            return Health;
        }
    }

    public interface IEnemyFactory
    {
        Enemy? CreateEnemy(int roomId);
        Mimic CreateMimic(int roomId, Chest chest);
    }
    public class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyIdFactory EnemyIdFactory;
        private Random Random = Random.Shared;

        private int MultiplicatorDivider = 100;

        private const int NoneMax = 60; // 60/100
        private const int SkeletorMax = 77; // 17/100
        private const int SkeletorArcherMax = 89; // 12/100
        private const int DeadmanMax = 95; // 7/100
        private const int GhostMax = 99; // 4/100
        //private const int LichMax = 100; // 1/100
        public EnemyFactory(IEnemyIdFactory enemyIdFactory)
        {
            EnemyIdFactory = enemyIdFactory;
        }
        public Enemy? CreateEnemy(int roomId)
        {
            double multiplicator = 1 + (roomId / MultiplicatorDivider);
            int number = (int)Math.Round(Random.Next(100) * multiplicator);
            return number switch
            {
                >= 0 and < NoneMax => null,
                >= NoneMax and < SkeletorMax => new Skeletor(roomId, EnemyIdFactory),
                >= SkeletorMax and < SkeletorArcherMax => new SkeletorArcher(roomId, EnemyIdFactory),
                >= SkeletorArcherMax and < DeadmanMax => new Deadman(roomId, EnemyIdFactory),
                >= DeadmanMax and < GhostMax => new Ghost(roomId, EnemyIdFactory),
                >= GhostMax and < 100 => new Lich(roomId, EnemyIdFactory),

                _ => null,
            };
        }
        public Mimic CreateMimic(int roomId, Chest chest)
        {
            return new Mimic(roomId, chest, EnemyIdFactory);
        }
    }
    #endregion 
}