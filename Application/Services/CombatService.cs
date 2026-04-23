using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.DTO;
using TextGame.Domain.GameText;
using TextGame.Domain.Entities.GameObjects.Enemies;

namespace TextGame.Application.Services
{
    public class CombatService : ICombatService
    {
        private readonly IGameSessionService _gameSessionService;
        private readonly IGetEnemyService _getEnemyService;
        private readonly IGameInfoService _gameInfoService;
        public CombatService(
            IGameSessionService gameSessionService,
            IGetEnemyService getEnemyService,
            IGameInfoService gameInfoService
            )
        {
            _gameSessionService = gameSessionService;
            _getEnemyService = getEnemyService;
            _gameInfoService = gameInfoService;
        }
        public BattleLog DealDamage()
        {
            int playerHealthBeforeAttack = _gameSessionService.CurrentHealth;
            Enemy enemy = _getEnemyService.GetEnemy();
            //attack
            var attackResult = _gameSessionService.Weapon.Attack(_gameSessionService.CurrentRoom!.Id);
            if (attackResult.SelfDamage != 0) _gameSessionService.AddCurrentHealth(-attackResult.SelfDamage);
            if (attackResult.IsWeaponBrokenDown) _gameSessionService.RemoveWeapon();

            int enemyHealthBeforeAttack = enemy.Health;
            int enemyHealthAfterAttack = enemy.GetDamage(attackResult.Damage);
            int playerHealthAfterAttack = playerHealthBeforeAttack - _gameSessionService.CurrentHealth;
            BattleLog battleLog = new BattleLog(enemy.Name!, attackResult.Damage, enemyHealthBeforeAttack, enemyHealthAfterAttack, GeneralLabeles.PlayerName, playerHealthAfterAttack, playerHealthBeforeAttack, _gameSessionService.CurrentHealth);

            if (enemyHealthAfterAttack <= 0)
            {
                _gameSessionService.RemoveEnemyFromCurrentRoom(enemy);
                if (_gameSessionService.CurrentMimicChest is not null)
                {
                    _gameSessionService.CurrentMimicChest.KillMimic();
                    _gameSessionService.AddItemToCurrentRoom(_gameSessionService.CurrentMimicChest);
                    _gameSessionService.RemoveCurrentMimicChest();
                }
                CheckPlayerHealthAfterAttack();
                if (_gameSessionService.CurrentRoom.Enemy == null) _gameSessionService.EndBattle();
                throw new BattleWinException(string.Format(ExceptionLabels.EnemyDefeated, enemy.Name), battleLog);
            }
            CheckPlayerHealthAfterAttack();
            return battleLog;
        }
        private void CheckPlayerHealthAfterAttack()
        {
            if (_gameSessionService.CurrentHealth <= 0) throw new DefeatException(ExceptionLabels.SuicideText, _gameInfoService.GetGameInfo());
        }
        public BattleLog GetDamage()
        {
            Enemy enemy = _getEnemyService.GetEnemy();
            int enemyHealthBeforeAttack = enemy.Health;
            int damage = enemy.Attack();

            //block
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

            int damageAfterBlock = damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = _gameSessionService.CurrentHealth;
            if (damageAfterBlock > 0) _gameSessionService.AddCurrentHealth(-damageAfterBlock);
            if (_gameSessionService.CurrentHealth <= 0) throw new DefeatException(string.Format(ExceptionLabels.PlayerDefeated, enemy.Name), _gameInfoService.GetGameInfo());
            int enemyHealthAfterAttack = enemyHealthBeforeAttack - enemy.Health;
            return new BattleLog(GeneralLabeles.PlayerName, damage, playerHealthBeforeAttack, _gameSessionService.CurrentHealth, enemy.Name!, enemyHealthAfterAttack, enemyHealthBeforeAttack, enemy.Health);
        }
    }
}