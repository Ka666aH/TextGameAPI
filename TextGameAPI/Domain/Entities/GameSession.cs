using System.Text.Json.Serialization;
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
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastSavedAt { get; private set; } = DateTime.UtcNow;
        public GameSessionState State { get; private set; } = new();
        public GameSession(Guid userId, string name = GameText.GeneralLabeles.GameSessionDefaultName)
        {
            UserId = userId;
            Name = name;
        }
        private GameSession() { }
        public void UpdateLastSavedAt() => LastSavedAt = DateTime.UtcNow;
        public void SetState(GameSessionState newState) => State = newState;
    }
    public class GameSessionState
    {
        public bool IsGameStarted { get; set; } = true;
        public bool IsInBattle { get; set; } = false;
        public List<Room> Rooms { get; set; } = [];
        public int CurrentRoomId { get; set; } = 0;
        public Weapon Weapon { get; set; } = GameBalance.StartWeapon;
        public Helm? Helm { get; set; } = GameBalance.StartHelm;
        public Chestplate? Chestplate { get; set; } = GameBalance.StartChestplate;
        public int MaxHealth { get; set; } = GameBalance.DefaultMaxHealth;
        public int CurrentHealth { get; set; } = GameBalance.DefaultMaxHealth;
        public int Coins { get; set; } = GameBalance.StartCoins;
        public int Keys { get; set; } = GameBalance.StartKeys;
        public List<Item> Inventory { get; set; } = [];
        public Chest? CurrentMimicChest { get; set; } = null;
    }
}