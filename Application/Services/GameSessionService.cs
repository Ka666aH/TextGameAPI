using TextGame.Application.Interfaces.Services;
using TextGame.Domain;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.GameObjects.Rooms;
using TextGame.Application.Interfaces.Generators;
using TextGame.Domain.GameObjects.Items.Other;

namespace TextGame.Application.Services
{
    public class GameSessionService : IGameSessionService
    {
        private GameSession _session;
        private readonly IMapGenerator _mapGenerator;
        public GameSessionService(GameSession session, IMapGenerator mapGenerator)
        {
            _session = session;
            _mapGenerator = mapGenerator;
        }
        public bool IsGameStarted => _session.IsGameStarted;
        public bool IsInBattle { get => _session.IsInBattle; }
        public Room CurrentRoom { get => _session.CurrentRoom!; }
        public IReadOnlyList<Room> Rooms => _session.Rooms.AsReadOnly();
        public IReadOnlyList<Item> Inventory => _session.Inventory.AsReadOnly();
        public int Coins { get => _session.Coins; }
        public int Keys { get => _session.Keys; }
        public Weapon Weapon { get => _session.Weapon; }
        public Helm? Helm { get => _session.Helm; }
        public Chestplate? Chestplate { get => _session.Chestplate; }
        public int MaxHealth { get => _session.MaxHealth; }
        public int CurrentHealth { get => _session.CurrentHealth; }
        public Chest? CurrentMimicChest { get => _session.CurrentMimicChest; }
        //public int RoomCounter { get => _session.RoomCounter; }
        //public int ItemCounter { get => _session.ItemCounter; }
        //public int EnemyCounter { get => _session.EnemyCounter; }


        //public int NextRoomNumber() => ++_session.RoomCounter;
        //public int NextItemId() => ++_session.ItemCounter;
        //public int NextEnemyId() => ++_session.EnemyCounter;


        public void RemoveWeapon() => _session.Weapon = Fists.DefaultFists;
        public void EquipWeapon(Weapon weapon) => _session.Weapon = weapon;
        public void RemoveChestplate() => _session.Chestplate = null;
        public void EquipChestplate(Chestplate chestplate) => _session.Chestplate = chestplate;
        public void RemoveHelm() => _session.Helm = null;
        public void EquipHelm(Helm helm) => _session.Helm = helm;

        public void AddMaxHealth(int value)
        {
            if (value == 0) return;

            _session.MaxHealth += value;
            _session.CurrentHealth += value;
        }
        public void AddCurrentHealth(int value)
        {
            if (value == 0) return;

            if ((CurrentHealth + value) >= MaxHealth) _session.CurrentHealth = MaxHealth;
            else _session.CurrentHealth += value;
        }
        public void RemoveCurrentMimicChest() => _session.CurrentMimicChest = null;
        public void SetCurrentMimicChest(Chest chest) => _session.CurrentMimicChest = chest;

        public void StartGame()
        {
            _session.Coins = 0;
            _session.Keys = 0;

            RemoveWeapon();
            RemoveHelm();
            RemoveChestplate();

            _session.Inventory = new List<Item>();
            _session.Rooms = new List<Room>();

            _session.MaxHealth = GameBalance.DefaultMaxHealth;
            _session.CurrentHealth = GameBalance.DefaultMaxHealth;


            //_session.RoomCounter = 0;
            //_session.ItemCounter = 0;
            //_session.EnemyCounter = 0;

            _session.Rooms = _mapGenerator.Generate();
            SetCurrentRoom(Rooms[0]);

            EndBattle();
            _session.IsGameStarted = true;
        }
        public void EndGame() => _session.IsGameStarted = false;
        public void StartBattle()
        {
            if (CurrentRoom.Enemies.Any()) _session.IsInBattle = true;
        }
        public void EndBattle()
        {
            if (!CurrentRoom.Enemies.Any()) _session.IsInBattle = false;
        }
        public void AddCoins(int value) => _session.Coins += value;
        public void AddKeys(int value) => _session.Keys += value;
        public void AddItemToInventory(Item item) => _session.Inventory.Add(item);
        public void RemoveItemFromInventory(Item item) => _session.Inventory.Remove(item);

        public void SetCurrentRoom(Room room) => _session.CurrentRoom = room;
        public void AddEnemyToCurrentRoom(Enemy enemy) => _session.CurrentRoom!.AddEnemy(enemy);
        public void RemoveEnemyFromCurrentRoom(Enemy enemy) => _session.CurrentRoom!.RemoveEnemy(enemy);
        public void AddItemToCurrentRoom(Item item) => _session.CurrentRoom!.AddItem(item);
        public void RemoveItemFromCurrentRoom(Item item) => _session.CurrentRoom!.RemoveItem(item);

        public List<Item> SearchCurrentRoom() => CurrentRoom!.Search();
    }
}