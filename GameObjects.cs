using System;

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
        public int Number { get; init; }
        public List<Item> Items { get; set; } = new List<Item>();
        public bool IsDiscovered { get; set; } = false;
        public Room(string name, string description, int number)
        {
            Name = name;
            Description = description;
            Number = number;
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
        public EmptyRoom(IRoomNumberFactory roomNumberFactory) : base("ПУСТАЯ КОМНАТА", "Ничего интересного.", roomNumberFactory.GetRoomNumber()) { }
    }
    public class SmallRoom : Room
    {

        public SmallRoom(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory) : base("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", roomNumberFactory.GetRoomNumber())
        {
            Item? item = itemFactory.CreateRoomItem();
            if (item != null) Items.Add(item);
        }
    }
    public class BigRoom : Room
    {
        public BigRoom(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory) : base("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", roomNumberFactory.GetRoomNumber())
        {
            const int itemsAmount = 3;
            for (int i = 0; i < itemsAmount; i++)
            {
                Item? item = itemFactory.CreateRoomItem();
                if (item != null) Items.Add(item);
            }
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
        private readonly Random random = Random.Shared;

        public RoomFactory(IRoomNumberFactory roomNumberFactory, IItemFactory itemFactory)
        {
            RoomNumberFactory = roomNumberFactory;
            ItemFactory = itemFactory;
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
                RoomType.SmallRoom => new SmallRoom(RoomNumberFactory, ItemFactory),
                RoomType.BigRoom => new BigRoom(RoomNumberFactory, ItemFactory),
                RoomType.EndRoom => new EndRoom(RoomNumberFactory),
                RoomType.Shop => new Shop(RoomNumberFactory, ItemFactory),
                _ => new EmptyRoom(RoomNumberFactory),
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
        public bool IsMimic { get; init; }
        public List<Item> Items { get; set; }

        private readonly int MinChestItemsAmount = 1;
        private readonly int MaxChestItemsAmount = 3;

        private readonly int MimicProbabilityDivider = 2; // 1/2
        private readonly int LockedProbabilityDivider = 2; // 1/2


        public Chest(IItemIdFactory itemIdFactory, IItemFactory itemFactory) : base("СУНДУК", "Хранит предметы. Может оказаться мимиком.", itemIdFactory!.Id(), false)
        {
            var random = new Random();
            IsLocked = random.Next(LockedProbabilityDivider) == 0;
            IsMimic = random.Next(MimicProbabilityDivider) == 0;
            Items = new List<Item>();
            var itemsInChest = random.Next(MinChestItemsAmount, MaxChestItemsAmount + 1);
            for (int i = 0; i < itemsInChest; i++)
            {
                Item? item = itemFactory.CreateChestItem();
                if (item != null) Items.Add(item);
            }
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public List<Item> Search() => Items;
    }
    #endregion
    #region Map
    public class Map : Item
    {
        public Map(IItemIdFactory itemIdFactory) : base("КАРТА", "Содержит знания о строении подземелья.", itemIdFactory!.Id(), true) { }
    }
    #endregion
    #region Equipment
    public abstract class Equipment : Item
    {
        public int? Durability;
        public Equipment(string? name, string? description, int? id, int? durability) : base(name, description, id, true) { }
    }
    #region Weapon
    public abstract class Weapon : Equipment
    {
        public int? Damage;
        public Weapon(string? name, string? description, int? id, int? durability, int? damage) : base(name, description, id, durability) { }
        public abstract int Attack(GameSession gameSession);
    }
    public class Fists : Weapon
    {
        public static readonly Fists DefaultFists = new Fists();
        public static readonly int SelfHarmProbabilityDivider = 2;
        public Fists() : base("КУЛАКИ", "То, что есть (почти) у каждого. Базовое оружие самозащиты. Может быть больно.", null, null, 1) { }
        public override int Attack(GameSession gameSession)
        {
            Random random = new Random();
            if (random.Next(SelfHarmProbabilityDivider) == 0) gameSession.CurrentHealth--;
            return (int)Damage!;
        }
    }
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
        private const int GlassSwordMax = 100;
        public Sword(IItemIdFactory itemIdFactory) : base(null, null, itemIdFactory.Id(), null, null)
        {

            Random random = new Random();
            int swordTypeNumber = random.Next(100);
            SwordType = swordTypeNumber switch
            {
                >= 0 and < RustSwordMax => SwordType.Rust,
                >= RustSwordMax and < IronSwordMax => SwordType.Iron,
                >= IronSwordMax and < SilverSwordMax => SwordType.Silver,
                >= SilverSwordMax and < GlassSwordMax => SwordType.Glass,

                _ => SwordType.Rust,
            };
            switch (SwordType)
            {
                case SwordType.Rust:
                    InitializeSword("РЖАВЫЙ МЕЧ", "Очень старый меч. Лучше, чем ничего.", random.Next(1, 21), random.Next(3, 8));
                    break;
                case SwordType.Iron:
                    InitializeSword("ЖЕЛЕЗНЫЙ МЕЧ", "Добротное оружие воина.", random.Next(1, 101), random.Next(8, 17));
                    break;
                case SwordType.Silver:
                    InitializeSword("СЕРЕБРЯНЫЙ МЕЧ", "Редкий меч из особого серебряного сплава. Эффективный, но менее прочный.", random.Next(10, 51), random.Next(25, 31));
                    break;
                case SwordType.Glass:
                    InitializeSword("СТЕКЛЯННЫЙ МЕЧ", "Скорее объект искусства, чем оружие. Очень хрупкий, но невероятно сильный.", 1, 100);
                    break;
            }
        }
        private void InitializeSword(string name, string description, int durability, int damage)
        {
            Name = name;
            Description = description;
            Durability = durability;
            Damage = damage;
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
    #region Armor
    public abstract class Armor : Equipment
    {
        public Armor(string name, string description, int id, int durability) : base(name, description, id, durability) { }
    }
    #region Helm
    public abstract class Helm : Equipment
    {
        public Helm(string name, string description, int id, int durability) : base(name, description, id, durability) { }
    }
    #endregion
    #region Helm
    public abstract class Chestplate : Equipment
    {
        public Chestplate(string name, string description, int id, int durability) : base(name, description, id, durability) { }
    }
    #endregion
    #endregion
    #endregion
    #region ItemId
    public interface IItemIdFactory
    {
        int Id();
        void Reset();
    }
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
    public enum RoomItem
    {
        None,
        Key,
        Coin,
        Chest,
    }
    public enum ChestItem
    {
        None,
        Key,
        Coin,
        Map,
    }

    public interface IItemFactory
    {
        public Item? CreateRoomItem();
        public Item? CreateChestItem();
        //shop
    }
    public class ItemFactory : IItemFactory
    {
        private readonly IItemIdFactory ItemIdFactory;
        private static readonly ChestItem[] ChestItemValues = (ChestItem[])Enum.GetValues(typeof(ChestItem));
        private static readonly RoomItem[] RoomItemValues = (RoomItem[])Enum.GetValues(typeof(RoomItem));
        private Random Random = Random.Shared;
        public ItemFactory(IItemIdFactory itemIdFactory)
        {
            ItemIdFactory = itemIdFactory;
        }
        public Item? CreateRoomItem()
        {
            var itemNumber = Random.Next(RoomItemValues.Length);
            RoomItem roomItem = RoomItemValues[itemNumber];
            return roomItem switch
            {
                RoomItem.Key => new Key(ItemIdFactory),
                RoomItem.Coin => new Coin(ItemIdFactory),
                RoomItem.Chest => new Chest(ItemIdFactory, this),
                _ => null,
            };
        }
        public Item? CreateChestItem()
        {
            var itemNumber = Random.Next(ChestItemValues.Length);
            ChestItem chestItem = ChestItemValues[itemNumber];
            return chestItem
                switch
            {
                ChestItem.Key => new Key(ItemIdFactory),
                ChestItem.Coin => new Coin(ItemIdFactory),
                ChestItem.Map => new Map(ItemIdFactory),
                _ => null,
            };
        }

    }
    #endregion

}
#endregion