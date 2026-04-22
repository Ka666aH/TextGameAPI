using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Domain.Entities
{
    public class GameSession
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }

        public List<Room> Rooms { get; set; } = [];
        public Room? CurrentRoom { get; set; }
        public Weapon Weapon { get; set; } = Fists.DefaultFists;
        public Helm? Helm { get; set; }
        public Chestplate? Chestplate { get; set; }
        public int MaxHealth { get; set; } = GameBalance.DefaultMaxHealth;
        public int CurrentHealth { get; set; } = GameBalance.DefaultMaxHealth;
        public int Coins { get; set; }
        public int Keys { get; set; }
        public List<Item> Inventory { get; set; } = [];
        public bool IsGameStarted { get; set; }
        public bool IsInBattle { get; set; } = false;
        public Chest? CurrentMimicChest { get; set; } = null;
        private GameSession() { }
        public GameSession(Guid userId)
        {
            UserId = userId;
        }
    }

}