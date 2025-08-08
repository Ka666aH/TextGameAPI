namespace TextGame
{
    public class GameObject
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
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
        public bool IsDiscovered {  get; set; } = false;
        public Room(string name, string description, int number)
        {
            Name = name;
            Description = description;
            Number = number;
        }
    }
    public class StartRoom : Room
    {
        public StartRoom() : base("СТАРТОВАЯ КОМАНТА", "В потолке дыра через которую вы сюда провалились.", 0) 
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
        private readonly IRoomItemFactory roomItemFactory;

        public SmallRoom(IRoomNumberFactory roomNumberFactory, IRoomItemFactory roomItemFactory) : base("МАЛЕНЬКАЯ КОМНАТА", "Тесная комната. Внутри может быть предмет.", roomNumberFactory.GetRoomNumber())
        {
            this.roomItemFactory = roomItemFactory;

            Item? item = roomItemFactory.CreateItem();
            if (item != null) Items.Add(item);
        }
    }
    public class BigRoom : Room
    {
        private readonly IRoomItemFactory roomItemFactory;

        public BigRoom(IRoomNumberFactory roomNumberFactory, IRoomItemFactory roomItemFactory) : base("БОЛЬШАЯ КОМНАТА", "Просторная комната. Внутри может быть до трёх предметов.", roomNumberFactory.GetRoomNumber())
        {
            this.roomItemFactory = roomItemFactory;

            const int itemsAmount = 3;
            for (int i = 0; i < itemsAmount; i++)
            {
                Item? item = roomItemFactory.CreateItem();
                if (item != null) Items.Add(item);
            }
        }
    }
    public class Shop : Room
    {
        private readonly IRoomItemFactory roomItemFactory;

        public Shop(IRoomNumberFactory roomNumberFactory, IRoomItemFactory roomItemFactory) : base("МАГАЗИН", "Здесь мутный торгаш продаёт своё добро.", roomNumberFactory.GetRoomNumber())
        {
            this.roomItemFactory = roomItemFactory;
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
        private readonly IRoomNumberFactory roomNumberFactory;
        private readonly IRoomItemFactory roomItemFactory;
        private readonly Random random = Random.Shared;

        public RoomFactory(IRoomNumberFactory roomNumberFactory, IRoomItemFactory roomItemFactory)
        {
            this.roomNumberFactory = roomNumberFactory;
            this.roomItemFactory = roomItemFactory;
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
                RoomType.SmallRoom => new SmallRoom(roomNumberFactory, roomItemFactory),
                RoomType.BigRoom => new BigRoom(roomNumberFactory, roomItemFactory),
                RoomType.EndRoom => new EndRoom(roomNumberFactory),
                RoomType.Shop => new Shop(roomNumberFactory, roomItemFactory),
                _ => new EmptyRoom(roomNumberFactory),
            };
        }
    }
    #endregion
    #region Item
    public class Item : GameObject
    {
        public int Id { get; init; }
        public bool IsCarryable { get; init; }
        public int Cost { get; init; }
    }
    public enum ItemType
    {
        None,
        Key,
        Coin,
        Chest,
    }

    #region Key
    //public enum KeyType
    //{
    //    Door,
    //    Chest,
    //    Master
    //}
    public class Key : Item
    {
        //public KeyType keyType { get; init; }

        //Random random = new Random();
        //KeyType[] keyTypes = (KeyType[])Enum.GetValues(typeof(KeyType));
        private readonly IItemIdFactory itemIdFactory;
        public Key(IItemIdFactory itemIdFactory)
        {
            this.itemIdFactory = itemIdFactory;

            Id = itemIdFactory!.Id(ItemType.Key);
            Name = "КЛЮЧ";
            Description = "Что-то открывает.";
            IsCarryable = true;
            //var randomType = random.Next(keyTypes.Length);
            //keyType = keyTypes[randomType];
        }
    }
    #endregion
    #region Coin
    public class Coin : Item
    {
        private readonly IItemIdFactory itemIdFactory;
        public Coin(IItemIdFactory itemIdFactory)
        {
            this.itemIdFactory = itemIdFactory;

            Id = itemIdFactory!.Id(ItemType.Coin);
            Name = "МОНЕТА";
            Description = "Блестит. Она явно ценная.";
            IsCarryable = true;
        }
    }
    #endregion
    #region Chest
    public class Chest : Item
    {
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; } = true;
        public bool IsMimic { get; init; }
        public List<Item> Items { get; set; }
        private readonly IChestItemFactory chestItemFactory;
        private readonly IItemIdFactory itemIdFactory;

        public Chest(IChestItemFactory chestItemFactory, IItemIdFactory itemIdFactory)
        {
            this.chestItemFactory = chestItemFactory;
            this.itemIdFactory = itemIdFactory;

            Id = itemIdFactory!.Id(ItemType.Chest);
            Name = "СУНДУК";
            Description = "Хранит предметы. Может оказаться мимиком.";
            IsCarryable = false;
            var random = new Random();
            IsLocked = random.Next(2) == 0 ? false : true;
            IsMimic = random.Next(2) == 0 ? false : true;
            Items = new List<Item>();
            var itemsInChest = random.Next(1, 4);
            for (int i = 0; i < itemsInChest; i++)
            {
                Item? item = chestItemFactory.CreateItem();
                if (item != null) Items.Add(item);
            }
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public List<Item> Search()
        { 
            return Items;
        }

    }
    #endregion
    #region ChestItems
    public enum ChestItem
    {
        None,
        Key,
        Coin,
    }

    public interface IChestItemFactory
    {
        public Item? CreateItem();
    }
    public class ChestItemFactory : IChestItemFactory
    {
        private readonly IItemIdFactory itemIdFactory;
        private static readonly ChestItem[] chestItemValues = (ChestItem[])Enum.GetValues(typeof(ChestItem));
        private Random random = new Random();

        public ChestItemFactory(IItemIdFactory itemIdFactory)
        {
            this.itemIdFactory = itemIdFactory;
        }
        public Item? CreateItem()
        {
            var itemNumber = random.Next(chestItemValues.Length);
            ChestItem chestItem = chestItemValues[itemNumber];
            return chestItem
                switch
            {
                ChestItem.Key => new Key(itemIdFactory),
                ChestItem.Coin => new Coin(itemIdFactory),
                _ => null,
            };
        }
    }
    #endregion
    #region RoomItems
    public enum RoomItem
    {
        None,
        Key,
        Coin,
        Chest,
    }

    public interface IRoomItemFactory
    {
        public Item? CreateItem();
    }
    public class RoomItemFactory : IRoomItemFactory
    {
        private readonly IChestItemFactory chestItemFactory;
        private readonly IItemIdFactory itemIdFactory;
        private static readonly RoomItem[] roomItemValues = (RoomItem[])Enum.GetValues(typeof(RoomItem));
        private Random random = new Random();

        public RoomItemFactory(IChestItemFactory chestItemFactory, IItemIdFactory itemIdFactory)
        {
            this.chestItemFactory = chestItemFactory;
            this.itemIdFactory = itemIdFactory;
        }

        public Item? CreateItem()
        {
            var itemNumber = random.Next(roomItemValues.Length);
            RoomItem roomItem = roomItemValues[itemNumber];
            return roomItem switch
            {
                RoomItem.Key => new Key(itemIdFactory),
                RoomItem.Coin => new Coin(itemIdFactory),
                RoomItem.Chest => new Chest(chestItemFactory, itemIdFactory),
                _ => null,
            };
        }
    }
    #endregion
    #region ItemId
    public interface IItemIdFactory
    {
        int Id(ItemType item);
        void Reset();
    }
    public class ItemIdFactory : IItemIdFactory
    {
        private int ItemId = 0;
        public int Id(ItemType item)
        {
            return Interlocked.Increment(ref ItemId);
        }
        public void Reset()
        {
            ItemId = 0;
        }
    }
    #endregion
}
#endregion