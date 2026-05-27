using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.DTO;
using TextGame.Domain.GameText;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Application.Enums;

namespace TextGame.Application.Services
{
    public class CombatService : ICombatService
    {
        private readonly IGameSessionStateService _gameSessionService;
        private readonly IGameInfoService _gameInfoService;
        public CombatService(
            IGameSessionStateService gameSessionService,
            IGameInfoService gameInfoService
            )
        {
            _gameSessionService = gameSessionService;
            _gameInfoService = gameInfoService;
        }
        public DealDamageOutcome DealDamage(out BattleLog battleLog)
        {
            int playerHealthBeforeAttack = _gameSessionService.CurrentHealth;
            Enemy enemy = _gameSessionService.CurrentEnemy;

            var attackResult = _gameSessionService.Weapon.Attack(_gameSessionService.CurrentRoom!.Id);
            if (attackResult.SelfDamage != 0) _gameSessionService.AddCurrentHealth(-attackResult.SelfDamage);
            if (attackResult.IsWeaponBrokenDown) _gameSessionService.RemoveWeapon();

            int enemyHealthBeforeAttack = enemy.Health;
            enemy.GetDamage(attackResult.Damage);
            int enemyHealthAfterAttack = enemy.Health;

            battleLog = new(
                enemy.Name,
                attackResult.Damage,
                enemyHealthBeforeAttack,
                enemyHealthAfterAttack,
                GeneralLabeles.PlayerName,
                attackResult.SelfDamage,
                playerHealthBeforeAttack,
                _gameSessionService.CurrentHealth);

            if (!IsPlayerAlive()) return DealDamageOutcome.PlayerDied;
            if (IsEnemyAlive(enemy)) return DealDamageOutcome.BattleContinues;

            _gameSessionService.RemoveEnemyFromCurrentRoom(enemy);
            if (MimicChestExists())
            {
                _gameSessionService.CurrentMimicChest!.KillMimic();
                _gameSessionService.AddItemToCurrentRoom(_gameSessionService.CurrentMimicChest);
                _gameSessionService.RemoveCurrentMimicChest();
            }
            return DealDamageOutcome.EnemyDefeated;
        }
        public GetDamageOutcome GetDamage(out BattleLog battleLog)
        {
            Enemy enemy = _gameSessionService.CurrentEnemy;
            int enemyHealthBeforeAttack = enemy.Health;
            var enemyAttackResult = enemy.Attack();

            int helmBlock = 0;
            if (_gameSessionService.Helm != null)
            {
                var blockResult = _gameSessionService.Helm.Block();
                helmBlock = blockResult.DamageBlock;
                if (blockResult.IsArmorBrokenDown) _gameSessionService.RemoveHelm();
            }

            int chestplateBlock = 0;
            if (_gameSessionService.Chestplate != null)
            {
                var blockResult = _gameSessionService.Chestplate.Block();
                chestplateBlock = blockResult.DamageBlock;
                if (blockResult.IsArmorBrokenDown) _gameSessionService.RemoveChestplate();
            }

            int damageAfterBlock = enemyAttackResult.Damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = _gameSessionService.CurrentHealth;
            if (damageAfterBlock > 0) _gameSessionService.AddCurrentHealth(-damageAfterBlock);
            battleLog = new BattleLog(
                GeneralLabeles.PlayerName,
                enemyAttackResult.Damage, 
                playerHealthBeforeAttack, 
                _gameSessionService.CurrentHealth, 
                enemy.Name, 
                enemyAttackResult.SelfDamage,
                enemyHealthBeforeAttack, 
                enemy.Health);

            if (_gameSessionService.CurrentHealth <= 0) return GetDamageOutcome.PlayerDefeated;
            return GetDamageOutcome.BattleContinues;
        }
        private bool IsPlayerAlive() => _gameSessionService.CurrentHealth > 0;
        private bool IsEnemyAlive(Enemy enemy) => enemy.Health > 0;
        private bool MimicChestExists() => _gameSessionService.CurrentMimicChest != null;
    }
}