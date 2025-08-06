using Microsoft.AspNetCore.Http;
namespace TextGame
{
    public class GameRepository : IGameRepository
    {
        private readonly IRoomNumberFactory roomNumberFactory;
        private readonly IRoomFactory roomFactory;
        private readonly IItemIdFactory itemIdFactory;

        // Состояние игры для текущей сессии
        public List<Room> Rooms { get; set; } = new List<Room>();
        public Room? CurrentRoom { get; set; }
        private List<Item> Inventory { get; set; } = new List<Item>();
        private int Coins { get; set; }
        private bool IsGameStarted { get; set; }

        public GameRepository(IRoomNumberFactory roomNumberFactory, IRoomFactory roomFactory, IItemIdFactory itemIdFactory)
        {
            this.roomNumberFactory = roomNumberFactory;
            this.roomFactory = roomFactory;
            this.itemIdFactory = itemIdFactory;
        }

        public void Start()
        {
            ResetGame();
            IsGameStarted = true;
            GoNextRoom();
        }
        public void ResetGame()
        {
            Coins = 0;
            Inventory = new List<Item>();
            Rooms = new List<Room>();
            itemIdFactory.Reset();
            roomNumberFactory.Reset();
        }

        public void GoNextRoom()
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            CurrentRoom = roomFactory.CreateRoom();
            Rooms.Add(CurrentRoom);
        }

        public List<Item> Search(int roomId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();
            Room room = GetRoomById(roomId);
            return room!.Items;
        }

        public void TakeItem(int roomId, int itemId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();
            if (!ItemIn(itemId, GetRoomById(roomId).Items)) throw new ArgumentNullException("item", "Предмет с таким ID не найден в комнате.");
            Item item = GetItemById(itemId, CurrentRoom!.Items);
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is Coin) Coins++;
            else Inventory.Add(item);
            CurrentRoom!.Items.Remove(item);
        }
        public void TakeAllItems(int roomId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();
            Room room = GetRoomById(roomId);
            if (room!.Items.Count(i => i.IsCarryable == true) == 0) throw new EmptyException();
            foreach (Item item in room.Items)
            {
                if (!item.IsCarryable) continue;
                if (item is Coin) Coins++;
                else Inventory.Add(item);
                //CurrentRoom.Items.Remove(item);
            }
            room.Items.RemoveAll(x => x.IsCarryable);
        }
        public bool ItemIn(int itemId, List<Item> items)
        {
            //Room room = GetRoomById(roomId);
            Item? item = items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new ArgumentNullException("item", "Предмет с таким ID не найден.");

            if (items.Contains(item)) return true;
            else return false;
        }
        public Room GetRoomById(int roomId)
        {
            Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            if (room == null) throw new ArgumentNullException("room", "Комната с таким номером не найдена.");
            return room;
        }
        public Item GetItemById(int itemId, List<Item> items)
        {
            Item? item = items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new ArgumentNullException("item", "Предмет с таким ID не найден.");
            return item;
        }
        public Item GetChestItemById(Chest chest, int itemId)
        {
            Item? item = chest.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new ArgumentNullException("item", "Предмет с таким ID не найден.");
            return item;
        }

        //public bool IsChestExist(int id)
        //{
        //    //return CurrentRoom.Items.Any(i => i.Id == id && i is Chest);
        //    return ValidateChestId(id);
        //}
        //public bool CheckChest(int id)
        //{
        //    if (!IsGameStarted) throw new UnstartedGameException();

        //    Chest chest = GetChestById(id);
        //    return chest.CheckChest();
        //}

        public void OpenChest(int roomId, int chestId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsMimic)
            {
                IsGameStarted = false;
                throw new MimicException();
            }
            chest.Open();
        }
        public ChestDTO ReturnChestDTO(Chest chest)
        {
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public ChestDTO ReturnChestDTO(int roomId,int chestId)
        {
            Chest chest = GetChestById(roomId,chestId);
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }

        public void UnlockChest(int roomId, int chestId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            Key? key = Inventory!.OfType<Key>().FirstOrDefault();
            if (key == null) throw new NoKeyException();
            Inventory.Remove(key);
            chest.Unlock();
        }

        public List<Item> SearchChest(int roomId, int chestId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsClosed) throw new ClosedException();
            return chest.Search();
        }
        public void TakeItemFromChest(int roomId, int chestId, int itemId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = GetItemById(itemId, chest.Items);
            if (item is Coin) Coins++;
            else Inventory.Add(item);
            chest.Items.Remove(item);
        }
        public void TakeAllItemsFromChest(int roomId, int chestId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            if (chest.Items.Count(i => i.IsCarryable == true) == 0) throw new EmptyException();
            foreach (Item item in chest.Items)
            {
                if (item is Coin) Coins++;
                else Inventory.Add(item);
                //chest.Items.Remove(item);
            }
            chest.Items.RemoveAll(x => x.IsCarryable);
        }
        public Chest GetChestById(int roomId, int chestId)
        {
            Room room = GetRoomById(roomId);
            Chest? chest = (Chest?)room!.Items.FirstOrDefault(i => i.Id == chestId);
            if (chest == null) throw new ArgumentNullException("chest", "Сундук с таким ID не найден.");
            if (chest is not Chest) throw new ArgumentException("Это не сундук.");
            return chest;
        }

        public CurrentRoomDTO ShowCurrentRoom()
        {
            return new CurrentRoomDTO(CurrentRoom!.Number, CurrentRoom!.Name!, CurrentRoom!.Description!);
        }
        
        public List<Item> ShowInventory()
        {
            return Inventory;
        }
        public Item ShowInventoryItem(int itemId)
        {
            return GetItemById(itemId, Inventory);
        }
        public List<Item> ShowInventoryItems(List<int> itemsIds)
        {
            List<Item> items = new List<Item>();
            foreach (var itemId in itemsIds)
            {
                items.Add(ShowInventoryItem(itemId));
            }
            return items;
        }
        public int ShowCoins()
        {
            return Coins;
        }
        public GameOverStatsDTO ShowGameOverStats()
        {
            return new(CurrentRoom!.Number, Coins,Inventory);
        }
        //private void EnsureGameStarted()
        //{
        //    if (!IsGameStarted || CurrentRoom == null)
        //    {
        //        Start();
        //    }
        //}
    }
    public class UnstartedGameException : Exception
    {
        public UnstartedGameException() : base("Игра ещё не начата!") { }
    }
    public class EmptyException : Exception
    {
        public EmptyException() : base("Тут ничего нет!") { }
    }
    public class UncarryableException : Exception
    {
        public UncarryableException() : base("Невозможно поднять этот предмет!") { }
    }
    public class LockedException : Exception
    {
        public LockedException() : base("Сундук заперт!") { }
    }
    public class NoKeyException : Exception
    {
        public NoKeyException() : base("Нет ключа!") { }
    }
    public class ClosedException : Exception
    {
        public ClosedException() : base("Сундук закрыт!") { }
    }
    public class MimicException : Exception
    {
        public MimicException() : base("НА ВАС НАПАЛ МИМИК! ВЫ ПОГИБЛИ!") { }
    }
}
