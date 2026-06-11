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
        private readonly IStateService _stateService;
        public CombatService(IStateService stateService)
        {
            _stateService = stateService;
        }
        public DealDamageOutcome DealDamage(out BattleLog battleLog)
        {
            int playerHealthBeforeAttack = _stateService.CurrentHealth;
            Enemy enemy = _stateService.CurrentEnemy!;

            var attackResult = _stateService.Weapon.Attack(_stateService.CurrentRoom!.Id);
            if (attackResult.SelfDamage != 0) _stateService.AddCurrentHealth(-attackResult.SelfDamage);
            if (attackResult.IsWeaponBrokenDown) _stateService.RemoveWeapon();

            int enemyHealthBeforeAttack = enemy.Health;
            enemy.GetDamage(attackResult.Damage);
            int enemyHealthAfterAttack = enemy.Health;

            battleLog = new(
                enemy.Name,
                attackResult.Damage,
                enemyHealthBeforeAttack,
                enemyHealthAfterAttack,
                GeneralLabels.PlayerName,
                attackResult.SelfDamage,
                playerHealthBeforeAttack,
                _stateService.CurrentHealth);

            if (!IsPlayerAlive()) return DealDamageOutcome.PlayerDied;
            if (IsEnemyAlive(enemy)) return DealDamageOutcome.BattleContinues;

            _stateService.RemoveEnemyFromCurrentRoom(enemy);
            if (MimicChestExists())
            {
                _stateService.CurrentMimicChest!.KillMimic();
                _stateService.AddItemToCurrentRoom(_stateService.CurrentMimicChest);
                _stateService.RemoveCurrentMimicChest();
            }
            return DealDamageOutcome.EnemyDefeated;
        }
        public GetDamageOutcome GetDamage(out BattleLog battleLog)
        {
            Enemy enemy = _stateService.CurrentEnemy!;
            int enemyHealthBeforeAttack = enemy.Health;
            var enemyAttackResult = enemy.Attack();

            int helmBlock = 0;
            if (_stateService.Helm != null)
            {
                var blockResult = _stateService.Helm.Block();
                helmBlock = blockResult.DamageBlock;
                if (blockResult.IsArmorBrokenDown) _stateService.RemoveHelm();
            }

            int chestplateBlock = 0;
            if (_stateService.Chestplate != null)
            {
                var blockResult = _stateService.Chestplate.Block();
                chestplateBlock = blockResult.DamageBlock;
                if (blockResult.IsArmorBrokenDown) _stateService.RemoveChestplate();
            }

            int damageAfterBlock = enemyAttackResult.Damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = _stateService.CurrentHealth;
            if (damageAfterBlock > 0) _stateService.AddCurrentHealth(-damageAfterBlock);
            battleLog = new BattleLog(
                GeneralLabels.PlayerName,
                enemyAttackResult.Damage, 
                playerHealthBeforeAttack, 
                _stateService.CurrentHealth, 
                enemy.Name, 
                enemyAttackResult.SelfDamage,
                enemyHealthBeforeAttack, 
                enemy.Health);

            if (_stateService.CurrentHealth <= 0) return GetDamageOutcome.PlayerDefeated;
            return GetDamageOutcome.BattleContinues;
        }
        private bool IsPlayerAlive() => _stateService.CurrentHealth > 0;
        private bool IsEnemyAlive(Enemy enemy) => enemy.Health > 0;
        private bool MimicChestExists() => _stateService.CurrentMimicChest != null;
    }
}