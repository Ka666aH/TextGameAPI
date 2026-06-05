using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Domain.Entities
{
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