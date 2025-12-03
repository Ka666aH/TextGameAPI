namespace TextGame
{
    public class GameSession
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public Room? CurrentRoom { get; set; }
        public Weapon Weapon { get; set; } = Fists.DefaultFists;
        public Helm? Helm { get; set; }
        public Chestplate? Chestplate { get; set; }
        public int MaxHealth { get; set; } = GameBalance.DefaultMaxHealth;
        public int CurrentHealth { get; set; } = GameBalance.DefaultMaxHealth;
        public int Coins { get; set; }
        public int Keys { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public bool IsGameStarted { get; set; }
        public bool IsInBattle { get; set; } = false;
        public Chest? CurrentMimicChest { get; set; } = null;

        public int RoomCounter { get; set; }
        public int ItemCounter { get; set; }
        public int EnemyCounter { get; set; }

        public int NextRoomNumber() => ++RoomCounter;
        public int NextItemId() => ++ItemCounter;
        public int NextEnemyId() => ++EnemyCounter;
    }
}