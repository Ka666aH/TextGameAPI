using TextGame.Domain;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Tests.TestSupport;

namespace TextGame.Tests.Domain;

/// <summary>
/// Enemy stat rolling and damage absorption. Production enemies roll stats through
/// Random.Shared, so every assertion is a range or an invariant, never an exact roll.
/// </summary>
public class EnemyTests
{
    [Fact]
    public void Skeletor_RollsHealthWithinStartRoomSpread()
    {
        // Arrange
        var (expectedMin, expectedMax) = GameBalance.CalculateSpread(GameBalance.SkeletorBaseHealth, 0);

        // Act
        var skeletor = new Skeletor(1, 0);

        // Assert
        skeletor.Health.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void Skeletor_RollsDamageWithinStartRoomSpread()
    {
        // Arrange
        var (expectedMin, expectedMax) = GameBalance.CalculateSpread(GameBalance.SkeletorBaseDamage, 0);

        // Act
        var skeletor = new Skeletor(1, 0);

        // Assert
        skeletor.Damage.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void Skeletor_RollsDamageBlockWithinStartRoomSpread()
    {
        // Arrange
        var (expectedMin, expectedMax) = GameBalance.CalculateSpread(GameBalance.SkeletorBaseDamageBlock, 0);

        // Act
        var skeletor = new Skeletor(1, 0);

        // Assert
        skeletor.DamageBlock.Should().BeInRange(expectedMin, expectedMax);
    }

    [Fact]
    public void Ghost_HasZeroDamageBlock()
    {
        // Arrange
        // (nothing to arrange)

        // Act
        var ghost = new Ghost(1, 0);

        // Assert
        ghost.DamageBlock.Should().Be(0);
    }

    [Fact]
    public void Attack_ReturnsEnemyDamage()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 7, damageBlock: 0);

        // Act
        EnemyAttackResult result = enemy.Attack();

        // Assert
        result.Should().Be(new EnemyAttackResult(7));
    }

    [Fact]
    public void GetDamage_WhenDamageEqualsBlock_LeavesHealthUnchanged()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 10, damageBlock: 5);

        // Act
        enemy.GetDamage(5);

        // Assert
        enemy.Health.Should().Be(100, "only the part of damage that exceeds the block is subtracted");
    }

    [Fact]
    public void GetDamage_WhenDamageExceedsBlock_SubtractsOnlyTheExcess()
    {
        // Arrange
        var enemy = new TestEnemy(health: 100, damage: 10, damageBlock: 5);

        // Act
        enemy.GetDamage(6);

        // Assert
        enemy.Health.Should().Be(99);
    }

    [Fact]
    public void GetDamage_WhenDamageIsFatal_LetsHealthGoNegative()
    {
        // Arrange
        var enemy = new TestEnemy(health: 10, damage: 10, damageBlock: 0);

        // Act
        enemy.GetDamage(100);

        // Assert
        enemy.Health.Should().Be(-90, "health is not clamped at zero, it is only compared with zero");
    }

    [Fact]
    public void GetDamage_WhenAlreadyDead_KeepsSubtracting()
    {
        // Arrange
        var enemy = new TestEnemy(health: 10, damage: 10, damageBlock: 0);
        enemy.GetDamage(100);

        // Act
        enemy.GetDamage(100);

        // Assert
        enemy.Health.Should().Be(-190);
    }

    [Fact]
    public void Ghost_GetDamage_AppliesEitherNothingOrFullDamage()
    {
        // Arrange
        const int damage = 5;
        const int attempts = 300;

        // Act + Assert
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            var ghost = new Ghost(1, 0);
            int healthBefore = ghost.Health;

            ghost.GetDamage(damage);

            bool untouched = ghost.Health == healthBefore;
            bool fullyDamaged = ghost.Health == healthBefore - damage;
            (untouched || fullyDamaged).Should().BeTrue("a ghost either dodges the whole hit or takes it whole");
        }
    }

    [Fact]
    public void Ghost_GetDamage_SometimesDealsFullDamage()
    {
        // Arrange
        const int damage = 1;
        const int attempts = 300;
        bool fullDamageAppliedAtLeastOnce = false;

        // Act
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            var ghost = new Ghost(1, 0);
            int healthBefore = ghost.Health;

            ghost.GetDamage(damage);

            if (ghost.Health == healthBefore - damage) fullDamageAppliedAtLeastOnce = true;
        }

        // Assert
        fullDamageAppliedAtLeastOnce.Should().BeTrue("a ghost never blocks damage, it only dodges it entirely");
    }
}
