using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;

namespace TextGame.Application.Services
{
    public class StateService : IStateService
    {
        private readonly ISessionProvider _sessionProvider;
        private readonly IStateCacheService _cache;
        private State? _state = null;
        public StateService(ISessionProvider sessionProvider, IStateCacheService cache)
        {
            _sessionProvider = sessionProvider;
            _cache = cache;
        }
        public async Task EnsureLoadedAsync(Guid sessionId, CancellationToken ct = default)
        {
            if (_state != null) return;
            _state = await _sessionProvider.GetAsync(sessionId, ct);
        }
        public async Task CacheAsync(Guid sessionId, CancellationToken ct = default)
        {
            await EnsureLoadedAsync(sessionId, ct);
            await _cache.SetAsync(sessionId, State, ct);
        }
        private State State => _state ?? throw new SessionNotFoundException();
        public bool IsGameStarted => State.IsGameStarted;
        public bool IsInBattle { get => State.IsInBattle; }
        public int CurrentRoomId { get => State.CurrentRoomId; }
        public Room CurrentRoom { get => State.Rooms[CurrentRoomId]; }
        public Enemy? CurrentEnemy { get => CurrentRoom.Enemy; }
        public IReadOnlyList<Room> Rooms => State.Rooms.AsReadOnly();
        public IReadOnlyList<Item> Inventory => State.Inventory.AsReadOnly();
        public int Coins { get => State.Coins; }
        public int Keys { get => State.Keys; }
        public Weapon Weapon { get => State.Weapon; }
        public Helm? Helm { get => State.Helm; }
        public Chestplate? Chestplate { get => State.Chestplate; }
        public int MaxHealth { get => State.MaxHealth; }
        public int CurrentHealth { get => State.CurrentHealth; }
        public Chest? CurrentMimicChest { get => State.CurrentMimicChest; }

        public void RemoveWeapon() => State.Weapon = Fists.DefaultFists;
        public void EquipWeapon(Weapon weapon) => State.Weapon = weapon;
        public void RemoveChestplate() => State.Chestplate = null;
        public void EquipChestplate(Chestplate chestplate) => State.Chestplate = chestplate;
        public void RemoveHelm() => State.Helm = null;
        public void EquipHelm(Helm helm) => State.Helm = helm;

        public void AddMaxHealth(int value)
        {
            if (value == 0) return;

            State.MaxHealth += value;
            State.CurrentHealth += value;
        }
        public void AddCurrentHealth(int value)
        {
            if (value == 0) return;

            if ((CurrentHealth + value) >= MaxHealth) State.CurrentHealth = MaxHealth;
            else State.CurrentHealth += value;
        }
        public void RemoveCurrentMimicChest() => State.CurrentMimicChest = null;
        public void SetCurrentMimicChest(Chest chest) => State.CurrentMimicChest = chest;

        public void EndGame() => State.IsGameStarted = false;
        public void StartBattle() => State.IsInBattle = true;
        public void EndBattle() => State.IsInBattle = false;
        public void AddCoins(int value) => State.Coins += value;
        public void AddKeys(int value) => State.Keys += value;
        public void AddItemToInventory(Item item) => State.Inventory.Add(item);
        public void RemoveItemFromInventory(Item item) => State.Inventory.Remove(item);

        public void SetCurrentRoom(int roomId) => State.CurrentRoomId = roomId;
        public void AddEnemyToCurrentRoom(Enemy enemy) => CurrentRoom.AddEnemy(enemy);
        public void RemoveEnemyFromCurrentRoom(Enemy enemy) => CurrentRoom.RemoveEnemy(enemy);
        public void AddItemToCurrentRoom(Item item) => CurrentRoom.AddItem(item);
        public void RemoveItemFromCurrentRoom(Item item) => CurrentRoom.RemoveItem(item);

        public IReadOnlyList<Item> SearchCurrentRoom() => CurrentRoom.Search();
    }
}