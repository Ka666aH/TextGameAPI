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
            CurrentRoom = room;
            return room!.Items;
        }

        public void TakeItem(int roomId, int itemId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();
            Room room = GetRoomById(roomId);
            if (!ItemIn(itemId, CurrentRoom!.Items)) throw new NullIdException("ITEM_IN_ROOM_NOT_FOUND", "Предмет с таким ID не найден в комнате.");
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
            Item? item = GetItemById(itemId, items);
            if (items.Contains(item)) return true;
            else return false;
        }
        public Room GetRoomById(int roomId)
        {
            //Room? room = Rooms.FirstOrDefault(r => r.Number == roomId);
            //if (room == null) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");

            if (roomId <= 0 || roomId > Rooms.Count) throw new NullIdException("ROOM_NOT_FOUND", "Комната с таким номером не найдена.");
            Room room = Rooms[roomId - 1];
            CurrentRoom = room;
            return room;
        }
        public Item GetItemById(int itemId, List<Item> items)
        {
            Item? item = items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new NullIdException("ITEM_NOT_FOUND", "Предмет с таким ID не найден.");
            return item;
        }
        public void OpenChest(int roomId, int chestId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(roomId, chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsMimic)
            {
                IsGameStarted = false;
                throw new DefeatException("НА ВАС НАПАЛ МИМИК! ВЫ БЫЛИ ПРОГЛОЧЕНЫ И ПЕРЕВАРЕНЫ!", ShowGameOverStats());
            }
            chest.Open();
        }
        public ChestDTO ReturnChestDTO(Chest chest)
        {
            return new ChestDTO(chest.Name!, chest.Description!, chest.IsLocked, chest.IsClosed);
        }
        public ChestDTO ReturnChestDTO(int roomId, int chestId)
        {
            Chest chest = GetChestById(roomId, chestId);
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
            if (chest == null) throw new NullIdException("CHEST_NOT_FOUND", "Сундук с таким ID не найден.");
            if (chest is not Chest) throw new InvalidIdException("NOT_CHEST", "Это не сундук.");
            return chest;
        }

        public CurrentRoomDTO ShowCurrentRoom()
        {
            if (!IsGameStarted && Rooms.Count <= 0) throw new UnstartedGameException();
            return new CurrentRoomDTO(CurrentRoom!.Number, CurrentRoom!.Name!, CurrentRoom!.Description!);
        }

        public List<Item> ShowInventory()
        {
            if (!IsGameStarted && Rooms.Count <= 0) throw new UnstartedGameException();
            return Inventory;
        }
        public Item ShowInventoryItem(int itemId)
        {
            if (!IsGameStarted && Rooms.Count <= 0) throw new UnstartedGameException();
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
            return new(CurrentRoom!.Number, Coins, Inventory);
        }
        //private void EnsureGameStarted()
        //{
        //    if (!IsGameStarted || CurrentRoom == null)
        //    {
        //        Start();
        //    }
        //}
    }
}
