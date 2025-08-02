using Microsoft.AspNetCore.Http;
namespace TextGame
{
    public class GameRepository : IGameRepository
    {
        private readonly IRoomNumberFactory roomNumberFactory;
        private readonly IRoomFactory roomFactory;
        private readonly IItemIdFactory itemIdFactory;

        // Состояние игры для текущей сессии
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
            CurrentRoom = roomFactory.CreateRoom();
            IsGameStarted = true;
        }
        public void ResetGame()
        {
            Coins = 0;
            Inventory = new List<Item>();
            itemIdFactory.Reset();
            roomNumberFactory.Reset();
        }

        public void GoNextRoom()
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            CurrentRoom = roomFactory.CreateRoom();
        }

        public List<Item> Search()
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            return CurrentRoom!.Items;
        }

        public void TakeItem(int id)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Item item = GetItemById(id);
            if (!item.IsCarryable) throw new UncarryableException();
            if (item is Coin) Coins++;
            else Inventory.Add(item);
            CurrentRoom!.Items.Remove(item);
        }
        public void TakeAllItems()
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            if (CurrentRoom!.Items.Count(i => i.IsCarryable == true) == 0) throw new EmptyException();
            foreach (Item item in CurrentRoom.Items)
            {
                if (!item.IsCarryable) continue;
                if (item is Coin) Coins++;
                else Inventory.Add(item);
                //CurrentRoom.Items.Remove(item);
            }
            CurrentRoom.Items.RemoveAll(x => x.IsCarryable);
        }
        public Item GetItemById(int id)
        {
            Item? item = CurrentRoom!.Items.FirstOrDefault(i => i.Id == id);
            if (item == null) throw new ArgumentNullException("Предмет с таким ID не найден.");
            return item;
        }
        public Item GetChestItemById(Chest chest, int itemId)
        {
            Item? item = chest.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new ArgumentNullException("Предмет с таким ID не найден.");
            return item;
        }

        //public bool IsChestExist(int id)
        //{
        //    //return CurrentRoom.Items.Any(i => i.Id == id && i is Chest);
        //    return ValidateChestId(id);
        //}
        public bool CheckChest(int id)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(id);
            return chest.CheckChest();
        }

        public bool UnlockChest(int id)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(id);
            Key? key = Inventory!.OfType<Key>().FirstOrDefault();
            if (key == null) return false;
            Inventory.Remove(key);
            chest.UnlockChest();
            return true;
        }

        public List<Item> OpenChest(int id)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(id);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsMimic)
            {
                IsGameStarted = false;
                throw new MimicException();
            }
            return chest.OpenChest();
        }
        public void TakeItemFromChest(int chestId, int itemId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(chestId);
            if (chest.IsLocked) throw new LockedException();
            if (chest.IsClosed) throw new ClosedException();
            Item item = GetChestItemById(chest, itemId);
            if (item is Coin) Coins++;
            else Inventory.Add(item);
            chest.Items.Remove(item);
        }
        public void TakeAllItemsFromChest(int chestId)
        {
            if (!IsGameStarted) throw new UnstartedGameException();

            Chest chest = GetChestById(chestId);
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
        public void ValidateChestId(int id)
        {
            Chest? chest = (Chest?)CurrentRoom!.Items.FirstOrDefault(i => i.Id == id);
            if (chest == null) throw new ArgumentNullException("Сундук с таким ID не найден.");
            if (chest is not Chest) throw new ArgumentException("Это не сундук.");
        }
        public Chest GetChestById(int id)
        {
            try
            {
                ValidateChestId(id);
                return (Chest)CurrentRoom!.Items.FirstOrDefault(i => i.Id == id)!;
            }
            catch
            {
                throw;
            }
        }

        public CurrentRoomDTO ShowCurrentRoom()
        {
            return new CurrentRoomDTO(CurrentRoom!.Number, CurrentRoom!.Name!, CurrentRoom!.Description!);
        }
        public record CurrentRoomDTO(int Number, string Name, string Description);
        public List<Item> ShowInventory()
        {
            return Inventory;
        }

        public int ShowCoins()
        {
            return Coins;
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
    public class ClosedException : Exception
    {
        public ClosedException() : base("Сундук закрыт!") { }
    }
    public class MimicException : Exception
    {
        public MimicException() : base("НА ВАС НАПАЛ МИМИК! ВЫ ПОГИБЛИ!") { }
    }
}
