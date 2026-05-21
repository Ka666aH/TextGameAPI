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
    public class GameSessionStateService : IGameSessionStateService
    {
        private readonly IGameSessionProvider _gameSessionProvider;
        private readonly IGameSessionStateCacheService _gameSessionCacheService;
        private GameSessionState? _gameSessionState = null;
        public GameSessionStateService(IGameSessionProvider gameSessionProvider, IGameSessionStateCacheService gameSessionCacheService)
        {
            _gameSessionProvider = gameSessionProvider;
            _gameSessionCacheService = gameSessionCacheService;
        }
        public async Task EnsureLoadedAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            if (_gameSessionState != null) return;
            _gameSessionState = await _gameSessionProvider.GetAsync(gameSessionId, ct);
        }
        public async Task CacheAsync(Guid gameSessionId, CancellationToken ct = default)
        {
            await EnsureLoadedAsync(gameSessionId, ct);
            await _gameSessionCacheService.SetAsync(gameSessionId, GameSessionState, ct);
        }
        private GameSessionState GameSessionState => _gameSessionState ?? throw new GameSessionNotFoundException();
        public bool IsGameStarted => GameSessionState.IsGameStarted;
        public bool IsInBattle { get => GameSessionState.IsInBattle; }
        public Room CurrentRoom { get => GameSessionState.CurrentRoom!; }
        public IReadOnlyList<Room> Rooms => GameSessionState.Rooms.AsReadOnly();
        public IReadOnlyList<Item> Inventory => GameSessionState.Inventory.AsReadOnly();
        public int Coins { get => GameSessionState.Coins; }
        public int Keys { get => GameSessionState.Keys; }
        public Weapon Weapon { get => GameSessionState.Weapon; }
        public Helm? Helm { get => GameSessionState.Helm; }
        public Chestplate? Chestplate { get => GameSessionState.Chestplate; }
        public int MaxHealth { get => GameSessionState.MaxHealth; }
        public int CurrentHealth { get => GameSessionState.CurrentHealth; }
        public Chest? CurrentMimicChest { get => GameSessionState.CurrentMimicChest; }

        public void RemoveWeapon() => GameSessionState.Weapon = Fists.DefaultFists;
        public void EquipWeapon(Weapon weapon) => GameSessionState.Weapon = weapon;
        public void RemoveChestplate() => GameSessionState.Chestplate = null;
        public void EquipChestplate(Chestplate chestplate) => GameSessionState.Chestplate = chestplate;
        public void RemoveHelm() => GameSessionState.Helm = null;
        public void EquipHelm(Helm helm) => GameSessionState.Helm = helm;

        public void AddMaxHealth(int value)
        {
            if (value == 0) return;

            GameSessionState.MaxHealth += value;
            GameSessionState.CurrentHealth += value;
        }
        public void AddCurrentHealth(int value)
        {
            if (value == 0) return;

            if ((CurrentHealth + value) >= MaxHealth) GameSessionState.CurrentHealth = MaxHealth;
            else GameSessionState.CurrentHealth += value;
        }
        public void RemoveCurrentMimicChest() => GameSessionState.CurrentMimicChest = null;
        public void SetCurrentMimicChest(Chest chest) => GameSessionState.CurrentMimicChest = chest;

        public void EndGame() => GameSessionState.IsGameStarted = false;
        public void StartBattle() => GameSessionState.IsInBattle = true;
        public void EndBattle() => GameSessionState.IsInBattle = false;
        public void AddCoins(int value) => GameSessionState.Coins += value;
        public void AddKeys(int value) => GameSessionState.Keys += value;
        public void AddItemToInventory(Item item) => GameSessionState.Inventory.Add(item);
        public void RemoveItemFromInventory(Item item) => GameSessionState.Inventory.Remove(item);

        public void SetCurrentRoom(Room room) => GameSessionState.CurrentRoom = room;
        public void AddEnemyToCurrentRoom(Enemy enemy) => GameSessionState.CurrentRoom!.AddEnemy(enemy);
        public void RemoveEnemyFromCurrentRoom(Enemy enemy) => GameSessionState.CurrentRoom!.RemoveEnemy(enemy);
        public void AddItemToCurrentRoom(Item item) => GameSessionState.CurrentRoom!.AddItem(item);
        public void RemoveItemFromCurrentRoom(Item item) => GameSessionState.CurrentRoom!.RemoveItem(item);

        public List<Item> SearchCurrentRoom() => CurrentRoom.Search();
    }
}