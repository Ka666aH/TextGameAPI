using TextGame.Application.Interfaces.Services;
using TextGame.Application.Orchestrators;
using TextGame.Application.Services;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameExceptions;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Application;

/// <summary>
/// Orchestrator scenarios. The room-entry, key and potion tests are regression guards for
/// notes/AGENTS.md §1.1, §2.2 and §2.3.
/// </summary>
public class StateOrchestratorTests
{
    [Fact]
    public void GoToRoom_WhenRoomIsDiscovered_MovesThePlayer()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var targetRoom = new EmptyRoom(id: 1);
        targetRoom.Discover();
        state.Rooms.Add(targetRoom);
        var stateService = new FakeStateService(state);
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Room room = orchestrator.GoToRoom(1);

        // Assert
        room.Should().BeSameAs(targetRoom);
        stateService.CurrentRoomId.Should().Be(1);
        stateService.IsGameStarted.Should().BeTrue();
    }

    [Fact]
    public void GoToRoom_WhenRoomIsUndiscovered_ThrowsUndiscoveredRoomException()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        state.Rooms.Add(new EmptyRoom(id: 1));
        var stateService = new FakeStateService(state);
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Action act = () => orchestrator.GoToRoom(1);

        // Assert
        // notes/AGENTS.md §1.1: no IsDiscovered check, so the player teleports into a
        // room they have not opened yet.
        act.Should().Throw<UndiscoveredRoomException>();
    }

    [Fact]
    public void GoToRoom_WhenIdIsOutsideTheMap_ThrowsRoomNotFoundException()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var stateService = new FakeStateService(state);
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Action act = () => orchestrator.GoToRoom(999);

        // Assert
        // notes/AGENTS.md §1.1: no range check, so the id reaches StateService and the
        // request dies with a raw index-out-of-range instead of a 404.
        act.Should().Throw<RoomNotFoundException>();
    }

    [Fact]
    public void GoToRoom_WhenRoomIsTheEndRoom_EndsTheGameAndThrowsWinException()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var endRoom = new EndRoom(id: 1);
        // The exit is only reachable through GoNextRoom, which discovers the room it enters,
        // so an undiscovered EndRoom never exists at runtime (notes/AGENTS.md §1.1).
        endRoom.Discover();
        state.Rooms.Add(endRoom);
        var stateService = new FakeStateService(state);
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Action act = () => orchestrator.GoToRoom(1);

        // Assert
        act.Should().Throw<WinException>();
        stateService.IsGameStarted.Should().BeFalse();
        stateService.Result.Should().Be(GameResult.Win);
    }

    [Fact]
    public void UnlockChest_WhenChestDoesNotExist_DoesNotSpendAKey()
    {
        // Arrange
        var state = new State { Keys = 1 };
        state.Rooms.Add(new StartRoom());
        var stateService = new FakeStateService(state);
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Action act = () => orchestrator.UnlockChest(chestId: 999);

        // Assert
        // notes/AGENTS.md §2.2: the key is spent before the chest is even looked up.
        act.Should().Throw<ItemNotFoundException>();
        stateService.Keys.Should().Be(1);
    }

    [Fact]
    public void UseInventoryItem_WhenPotionKillsThePlayer_EndsTheGame()
    {
        // Arrange
        var state = new State { CurrentHealth = 10, MaxHealth = 100 };
        state.Rooms.Add(new StartRoom());
        var stateService = new FakeStateService(state);
        var poisonedPotion = new ScriptedHeal(maxHealthBoost: 0, currentHealthBoost: -20);
        stateService.AddItemToInventory(poisonedPotion);
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Action act = () => orchestrator.UseInventoryItem(itemId: 0);

        // Assert
        // notes/AGENTS.md §2.3: DefeatException is thrown without EndGame, so the state
        // stays "started" with health at or below zero.
        act.Should().Throw<DefeatException>();
        stateService.IsGameStarted.Should().BeFalse();
        stateService.Result.Should().Be(GameResult.Lose);
    }

    [Fact]
    public void DealDamage_WhenSelfDamageKillsThePlayer_EndsTheGameWithLose()
    {
        // Arrange
        var state = new State { CurrentHealth = 5, MaxHealth = 100 };
        state.Rooms.Add(new StartRoom());
        var stateService = new FakeStateService(state);
        stateService.AddEnemyToCurrentRoom(new TestEnemy(health: 100));
        stateService.EquipWeapon(new ScriptedWeapon(damage: 1, selfDamage: 10));
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        Action act = () => orchestrator.DealDamage();

        // Assert
        act.Should().Throw<DefeatException>();
        stateService.IsGameStarted.Should().BeFalse();
        stateService.Result.Should().Be(GameResult.Lose);
    }

    [Fact]
    public void GetMap_WhenMapIsInInventory_ReturnsDiscoveryFlagForEveryRoom()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        state.Rooms.Add(new EmptyRoom(id: 1));
        var stateService = new FakeStateService(state);
        stateService.AddItemToInventory(new Map(id: 0, roomId: 0));
        var orchestrator = CreateOrchestrator(stateService);

        // Act
        var map = orchestrator.GetMap();

        // Assert
        map.Should().HaveCount(2);
        map[0].IsDiscovered.Should().BeTrue("стартовая комната открыта в конструкторе");
        map[1].IsDiscovered.Should().BeFalse();
    }

    [Fact]
    public void TakeAllItemsFromChest_WhenCheckFailsOnSecondItem_LeavesChestFull()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var chest = new Chest(id: 0, roomId: 0, items: [new BagOfCoins(1, 0), new BagOfCoins(2, 0)]);
        chest.IsLocked = false;
        chest.Open();
        var stateService = new FakeStateService(state);
        stateService.AddItemToCurrentRoom(chest);
        var orchestrator = CreateOrchestrator(stateService, new FailingOnSecondItemCheckService());

        // Act
        Action act = () => orchestrator.TakeAllItemsFromChest(chestId: 0);

        // Assert
        // notes/backend-requests.md п. 11: проверка до изъятия, поэтому отказ на втором
        // предмете не оставляет сундук пустым.
        act.Should().Throw<UncarryableException>();
        chest.Items.Should().HaveCount(2);
    }

    [Fact]
    public void TakeItemFromChest_WhenCheckFails_LeavesItemInChest()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var chest = new Chest(id: 0, roomId: 0, items: [new BagOfCoins(1, 0)]);
        chest.IsLocked = false;
        chest.Open();
        var stateService = new FakeStateService(state);
        stateService.AddItemToCurrentRoom(chest);
        var orchestrator = CreateOrchestrator(stateService, new AlwaysFailingCheckService());

        // Act
        Action act = () => orchestrator.TakeItemFromChest(chestId: 0, itemId: 1);

        // Assert
        act.Should().Throw<UncarryableException>();
        chest.Items.Should().ContainSingle();
    }

    [Fact]
    public void TakeItemFromChest_WhenChestIsLocked_RejectsBeforeCheckingTheItem()
    {
        // Arrange
        var state = new State();
        state.Rooms.Add(new StartRoom());
        var chest = new Chest(id: 0, roomId: 0, items: [new BagOfCoins(1, 0)]) { IsLocked = true };
        var stateService = new FakeStateService(state);
        stateService.AddItemToCurrentRoom(chest);
        var checkItemService = new FakeCheckItemService();
        var orchestrator = CreateOrchestrator(stateService, checkItemService);

        // Act
        Action act = () => orchestrator.TakeItemFromChest(chestId: 0, itemId: 1);

        // Assert
        // CheckItem кладёт предмет в инвентарь, поэтому отказ по замку обязан случиться до него.
        act.Should().Throw<LockedException>();
        checkItemService.CheckedItems.Should().BeEmpty();
        chest.Items.Should().ContainSingle();
    }

    private static StateOrchestrator CreateOrchestrator(
        FakeStateService stateService,
        ICheckItemService? checkItemService = null)
    {
        var getItemService = new GetItemService();

        return new StateOrchestrator(
            stateService,
            new FakeGameInfoService(),
            getItemService,
            new GetRoomService(stateService),
            new InventoryService(stateService),
            new ChestService(getItemService),
            new CombatService(stateService),
            checkItemService ?? new FakeCheckItemService());
    }

    private sealed class FailingOnSecondItemCheckService : ICheckItemService
    {
        private int _calls;

        public void CheckItem(Item item)
        {
            _calls++;
            if (_calls == 2) throw new UncarryableException();
        }
    }

    private sealed class AlwaysFailingCheckService : ICheckItemService
    {
        public void CheckItem(Item item) => throw new UncarryableException();
    }
}
