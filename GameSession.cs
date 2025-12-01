namespace TextGame
{
    public class GameSession
    {
        public List<Room> Rooms { get; set; } = new List<Room>()
            {
                new StartRoom(),
            };
        public Room? CurrentRoom { get; set; }
        public Weapon Weapon { get; set; } = Fists.DefaultFists;
        public Helm? Helm { get; set; }
        public Chestplate? Chestplate { get; set; }
        public const int DefaultMaxHealth = 100;
        public int MaxHealth { get; set; } = DefaultMaxHealth;
        public int CurrentHealth { get; set; } = DefaultMaxHealth;
        public int Coins { get; set; }
        public int Keys { get; set; }
        public List<Item> Inventory { get; set; } = new List<Item>();
        public bool IsGameStarted { get; set; }
        public bool IsInBattle { get; set; } = false;
        public Chest? CurrentMimicChest { get; set; } = null;
        public void RemoveWeapon()
        {
            Weapon = Fists.DefaultFists;
        }
        public void RemoveHelm()
        {
            Helm = null;
        }
        public void RemoveChestplate()
        {
            Chestplate = null;
        }
    }
}