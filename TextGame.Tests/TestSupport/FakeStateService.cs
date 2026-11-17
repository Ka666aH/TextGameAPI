using TextGame.Application.Interfaces.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Tests.TestSupport;

/// <summary>
/// In-memory <see cref="IStateService"/> that mutates a real <see cref="State"/>.
/// It copies the observable behaviour of the production StateService, including
/// its index-by-CurrentRoomId access, so tests see the same failures.
/// </summary>
internal sealed class FakeStateService : IStateService
{
    private readonly State _state;

    public FakeStateService(State? state = null) => _state = state ?? new State();

    public State State => _state;

    public Task EnsureLoadedAsync(Guid sessionId, CancellationToken ct = default) => Task.CompletedTask;

    public Task CacheAsync(Guid sessionId, CancellationToken ct = default) => Task.CompletedTask;

    public bool IsGameStarted => _state.IsGameStarted;
    public bool IsInBattle => _state.IsInBattle;
    public int CurrentRoomId => _state.CurrentRoomId;
    public Room CurrentRoom => _state.Rooms[_state.CurrentRoomId];
    public Enemy? CurrentEnemy => CurrentRoom.Enemy;
    public IReadOnlyList<Room> Rooms => _state.Rooms.AsReadOnly();
    public IReadOnlyList<Item> Inventory => _state.Inventory.AsReadOnly();
    public int Coins => _state.Coins;
    public int Keys => _state.Keys;
    public Weapon Weapon => _state.Weapon;
    public Helm? Helm => _state.Helm;
    public Chestplate? Chestplate => _state.Chestplate;
    public int MaxHealth => _state.MaxHealth;
    public int CurrentHealth => _state.CurrentHealth;
    public Chest? CurrentMimicChest => _state.CurrentMimicChest;

    public void RemoveWeapon() => _state.Weapon = Fists.DefaultFists;
    public void EquipWeapon(Weapon weapon) => _state.Weapon = weapon;
    public void RemoveChestplate() => _state.Chestplate = null;
    public void EquipChestplate(Chestplate chestplate) => _state.Chestplate = chestplate;
    public void RemoveHelm() => _state.Helm = null;
    public void EquipHelm(Helm helm) => _state.Helm = helm;

    public void AddMaxHealth(int value)
    {
        if (value == 0) return;

        _state.MaxHealth += value;
        _state.CurrentHealth += value;
    }

    public void AddCurrentHealth(int value)
    {
        if (value == 0) return;

        if (CurrentHealth + value >= MaxHealth) _state.CurrentHealth = MaxHealth;
        else _state.CurrentHealth += value;
    }

    public void RemoveCurrentMimicChest() => _state.CurrentMimicChest = null;
    public void SetCurrentMimicChest(Chest chest) => _state.CurrentMimicChest = chest;
    public void EndGame() => _state.IsGameStarted = false;
    public void StartBattle() => _state.IsInBattle = true;
    public void EndBattle() => _state.IsInBattle = false;
    public void AddCoins(int value) => _state.Coins += value;
    public void AddKeys(int value) => _state.Keys += value;
    public void AddItemToInventory(Item item) => _state.Inventory.Add(item);
    public void RemoveItemFromInventory(Item item) => _state.Inventory.Remove(item);
    public void SetCurrentRoom(int roomId) => _state.CurrentRoomId = roomId;
    public void AddEnemyToCurrentRoom(Enemy enemy) => CurrentRoom.AddEnemy(enemy);
    public void RemoveEnemyFromCurrentRoom(Enemy enemy) => CurrentRoom.RemoveEnemy(enemy);
    public void AddItemToCurrentRoom(Item item) => CurrentRoom.AddItem(item);
    public void RemoveItemFromCurrentRoom(Item item) => CurrentRoom.RemoveItem(item);
    public IReadOnlyList<Item> SearchCurrentRoom() => CurrentRoom.Search();
}
