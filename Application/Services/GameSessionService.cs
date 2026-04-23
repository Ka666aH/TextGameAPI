using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Application.Interfaces.Generators;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities;

namespace TextGame.Application.Services
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IGameSessionProvider _gameSessionProvider;
        private readonly IMapGenerator _mapGenerator;
        public GameSessionService(IMapGenerator mapGenerator, IGameSessionProvider gameSessionProvider)
        {
            _mapGenerator = mapGenerator;
            _gameSessionProvider = gameSessionProvider;
        }
        private GameSession GameSession => _gameSessionProvider.GetGameSession();
        public bool IsGameStarted => GameSession.IsGameStarted;
        public bool IsInBattle { get => GameSession.IsInBattle; }
        public Room CurrentRoom { get => GameSession.CurrentRoom!; }
        public IReadOnlyList<Room> Rooms => GameSession.Rooms.AsReadOnly();
        public IReadOnlyList<Item> Inventory => GameSession.Inventory.AsReadOnly();
        public int Coins { get => GameSession.Coins; }
        public int Keys { get => GameSession.Keys; }
        public Weapon Weapon { get => GameSession.Weapon; }
        public Helm? Helm { get => GameSession.Helm; }
        public Chestplate? Chestplate { get => GameSession.Chestplate; }
        public int MaxHealth { get => GameSession.MaxHealth; }
        public int CurrentHealth { get => GameSession.CurrentHealth; }
        public Chest? CurrentMimicChest { get => GameSession.CurrentMimicChest; }

        public void RemoveWeapon() => GameSession.Weapon = Fists.DefaultFists;
        public void EquipWeapon(Weapon weapon) => GameSession.Weapon = weapon;
        public void RemoveChestplate() => GameSession.Chestplate = null;
        public void EquipChestplate(Chestplate chestplate) => GameSession.Chestplate = chestplate;
        public void RemoveHelm() => GameSession.Helm = null;
        public void EquipHelm(Helm helm) => GameSession.Helm = helm;

        public void AddMaxHealth(int value)
        {
            if (value == 0) return;

            GameSession.MaxHealth += value;
            GameSession.CurrentHealth += value;
        }
        public void AddCurrentHealth(int value)
        {
            if (value == 0) return;

            if ((CurrentHealth + value) >= MaxHealth) GameSession.CurrentHealth = MaxHealth;
            else GameSession.CurrentHealth += value;
        }
        public void RemoveCurrentMimicChest() => GameSession.CurrentMimicChest = null;
        public void SetCurrentMimicChest(Chest chest) => GameSession.CurrentMimicChest = chest;

        public void StartGame()
        {
            GameSession.Coins = 0;
            GameSession.Keys = 0;

            RemoveWeapon();
            RemoveHelm();
            RemoveChestplate();

            GameSession.Inventory = [];
            GameSession.Rooms = [];

            GameSession.MaxHealth = GameBalance.DefaultMaxHealth;
            GameSession.CurrentHealth = GameBalance.DefaultMaxHealth;

            GameSession.Rooms = _mapGenerator.Generate();
            SetCurrentRoom(Rooms[0]);

            EndBattle();
            GameSession.IsGameStarted = true;
        }
        public void EndGame() => GameSession.IsGameStarted = false;
        public void StartBattle() => GameSession.IsInBattle = true;
        public void EndBattle() => GameSession.IsInBattle = false;
        public void AddCoins(int value) => GameSession.Coins += value;
        public void AddKeys(int value) => GameSession.Keys += value;
        public void AddItemToInventory(Item item) => GameSession.Inventory.Add(item);
        public void RemoveItemFromInventory(Item item) => GameSession.Inventory.Remove(item);

        public void SetCurrentRoom(Room room) => GameSession.CurrentRoom = room;
        public void AddEnemyToCurrentRoom(Enemy enemy) => GameSession.CurrentRoom!.AddEnemy(enemy);
        public void RemoveEnemyFromCurrentRoom(Enemy enemy) => GameSession.CurrentRoom!.RemoveEnemy(enemy);
        public void AddItemToCurrentRoom(Item item) => GameSession.CurrentRoom!.AddItem(item);
        public void RemoveItemFromCurrentRoom(Item item) => GameSession.CurrentRoom!.RemoveItem(item);

        public List<Item> SearchCurrentRoom() => CurrentRoom!.Search();
    }
}