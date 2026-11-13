using TextGame.Application.Enums;
using TextGame.Application.Services;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Domain.GameText;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Application;

/// <summary>
/// Combat resolution against a real <see cref="State"/> held by FakeStateService.
/// Weapons and enemies are scripted so every number is deterministic.
/// </summary>
public class CombatServiceTests
{
    [Fact]
    public void DealDamage_WhenEnemySurvives_ReturnsBattleContinues()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 10, damageBlock: 0);
        var (_, combat, room) = CreateCombat(new ScriptedWeapon(damage: 5), enemy);

        // Act
        DealDamageOutcome outcome = combat.DealDamage(out BattleLog log);

        // Assert
        outcome.Should().Be(DealDamageOutcome.BattleContinues);
        enemy.Health.Should().Be(95);
        room.Enemy.Should().BeSameAs(enemy);
        log.damageToTarget.Should().Be(5);
        log.targetHealthBeforeAttack.Should().Be(100);
        log.targetHealthAfterAttack.Should().Be(95);
        log.attacker.Should().Be(GeneralLabels.PlayerName);
    }

    [Fact]
    public void DealDamage_WhenEnemyDies_ReturnsEnemyDefeatedAndClearsTheRoom()
    {
        // Arrange
        var enemy = new TestEnemy(health: 5, damage: 1, damageBlock: 0);
        var (_, combat, room) = CreateCombat(new ScriptedWeapon(damage: 5), enemy);

        // Act
        DealDamageOutcome outcome = combat.DealDamage(out _);

        // Assert
        outcome.Should().Be(DealDamageOutcome.EnemyDefeated);
        room.Enemy.Should().BeNull();
    }

    [Fact]
    public void DealDamage_WhenWeaponBreaks_ReplacesItWithBareHands()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 1, damageBlock: 0);
        var (stateService, combat, _) = CreateCombat(new ScriptedWeapon(damage: 5, isWeaponBrokenDown: true), enemy);

        // Act
        combat.DealDamage(out _);

        // Assert
        stateService.Weapon.Should().BeSameAs(Fists.DefaultFists);
    }

    [Fact]
    public void DealDamage_WhenSelfDamageKillsThePlayer_ReturnsPlayerDied()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 1, damageBlock: 0);
        var (stateService, combat, _) = CreateCombat(new ScriptedWeapon(damage: 5, selfDamage: 100), enemy, playerHealth: 100);

        // Act
        DealDamageOutcome outcome = combat.DealDamage(out _);

        // Assert
        outcome.Should().Be(DealDamageOutcome.PlayerDied);
        stateService.CurrentHealth.Should().Be(0);
    }

    [Fact]
    public void DealDamage_WhenMimicChestWasOpened_KillsMimicAndReturnsChestToRoom()
    {
        // Arrange
        var chest = new Chest(id: 5, roomId: 0, items: new List<Item>(), mimic: new Mimic(id: 6, roomId: 0));
        var enemy = new TestEnemy(health: 5, damage: 1, damageBlock: 0);
        var (stateService, combat, room) = CreateCombat(new ScriptedWeapon(damage: 5), enemy);
        stateService.SetCurrentMimicChest(chest);

        // Act
        DealDamageOutcome outcome = combat.DealDamage(out _);

        // Assert
        outcome.Should().Be(DealDamageOutcome.EnemyDefeated);
        chest.Mimic.Should().BeNull();
        stateService.CurrentMimicChest.Should().BeNull();
        room.Items.Should().Contain(chest);
    }

    [Fact]
    public void GetDamage_WhenHelmBlocksEverything_LeavesHealthUnchanged()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 5, damageBlock: 0);
        var (stateService, combat, _) = CreateCombat(new ScriptedWeapon(damage: 1), enemy);
        stateService.EquipHelm(new ScriptedHelm(durability: 3, damageBlock: 5));

        // Act
        GetDamageOutcome outcome = combat.GetDamage(out BattleLog log);

        // Assert
        outcome.Should().Be(GetDamageOutcome.BattleContinues);
        stateService.CurrentHealth.Should().Be(100);
        stateService.Helm!.Durability.Should().Be(2);
        log.healthBeforeAttack.Should().Be(100);
        log.healthAfterAttack.Should().Be(100);
    }

    [Fact]
    public void GetDamage_WhenHelmBreaks_RemovesIt()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 5, damageBlock: 0);
        var (stateService, combat, _) = CreateCombat(new ScriptedWeapon(damage: 1), enemy);
        stateService.EquipHelm(new ScriptedHelm(durability: 1, damageBlock: 0));

        // Act
        combat.GetDamage(out _);

        // Assert
        stateService.Helm.Should().BeNull();
    }

    [Fact]
    public void GetDamage_WhenBlocksAreInsufficient_SubtractsOnlyTheRemainder()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 10, damageBlock: 0);
        var (stateService, combat, _) = CreateCombat(new ScriptedWeapon(damage: 1), enemy);
        stateService.EquipHelm(new ScriptedHelm(durability: 3, damageBlock: 3));
        stateService.EquipChestplate(new ScriptedChestplate(durability: 3, damageBlock: 4));

        // Act
        combat.GetDamage(out _);

        // Assert
        stateService.CurrentHealth.Should().Be(97);
    }

    [Fact]
    public void GetDamage_WhenHealthReachesZero_ReturnsPlayerDefeated()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 10, damageBlock: 0);
        var (stateService, combat, _) = CreateCombat(new ScriptedWeapon(damage: 1), enemy, playerHealth: 5);

        // Act
        GetDamageOutcome outcome = combat.GetDamage(out _);

        // Assert
        outcome.Should().Be(GetDamageOutcome.PlayerDefeated);
        stateService.CurrentHealth.Should().Be(-5);
    }

    private static (FakeStateService StateService, CombatService Combat, Room Room) CreateCombat(
        Weapon weapon,
        Enemy enemy,
        int playerHealth = 100,
        int playerMaxHealth = 100)
    {
        var state = new State
        {
            Weapon = weapon,
            CurrentHealth = playerHealth,
            MaxHealth = playerMaxHealth,
        };
        var room = new StartRoom();
        state.Rooms.Add(room);

        var stateService = new FakeStateService(state);
        room.AddEnemy(enemy);

        return (stateService, new CombatService(stateService), room);
    }
}
