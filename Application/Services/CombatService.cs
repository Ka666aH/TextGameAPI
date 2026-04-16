using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.DTO;

namespace TextGame.Application.Services
{
    public class CombatService : ICombatService
    {
        private readonly IGameSessionService _sessionService;
        private readonly IGetEnemyService _getEnemyService;
        private readonly IGameInfoService _gameInfoService;
        public CombatService(
            IGameSessionService sessionService,
            IGetEnemyService getEnemyService,
            IGameInfoService gameInfoService
            )
        {
            _sessionService = sessionService;
            _getEnemyService = getEnemyService;
            _gameInfoService = gameInfoService;
        }
        public BattleLog DealDamage()
        {
            int playerHealthBeforeAttack = _sessionService.CurrentHealth;
            Enemy enemy = _getEnemyService.GetEnemy();
            //attack
            var attackResult = _sessionService.Weapon.Attack(_sessionService.CurrentRoom!.Number);
            if (attackResult.SelfDamage != 0) _sessionService.AddCurrentHealth(-attackResult.SelfDamage);
            if (attackResult.IsWeaponBrokenDown) _sessionService.RemoveWeapon();

            int enemyHealthBeforeAttack = enemy.Health;
            int enemyHealthAfterAttack = enemy.GetDamage(attackResult.Damage);
            int playerHealthAfterAttack = playerHealthBeforeAttack - _sessionService.CurrentHealth;
            BattleLog battleLog = new BattleLog(enemy.Name!, attackResult.Damage, enemyHealthBeforeAttack, enemyHealthAfterAttack, "ИГРОК", playerHealthAfterAttack, playerHealthBeforeAttack, _sessionService.CurrentHealth);

            if (enemyHealthAfterAttack <= 0)
            {
                _sessionService.RemoveEnemyFromCurrentRoom(enemy);
                if (_sessionService.CurrentMimicChest is not null)
                {
                    _sessionService.CurrentMimicChest.KillMimic();
                    _sessionService.AddItemToCurrentRoom(_sessionService.CurrentMimicChest);
                    _sessionService.RemoveCurrentMimicChest();
                }
                CheckPlayerHealthAfterAttack();
                _sessionService.EndBattle();
                throw new BattleWinException($"{enemy.Name!} повержен.", battleLog);
            }
            CheckPlayerHealthAfterAttack();
            return battleLog;
        }
        private void CheckPlayerHealthAfterAttack()
        {
            if (_sessionService.CurrentHealth <= 0) throw new DefeatException("Вы погибли от своей же атаки. Как отчаянно.", _gameInfoService.GetGameInfo());
        }
        public BattleLog GetDamage()
        {
            Enemy enemy = _getEnemyService.GetEnemy();
            int enemyHealthBeforeAttack = enemy.Health;
            int damage = enemy.Attack();

            //block
            int helmBlock = 0;
            if (_sessionService.Helm != null)
            {
                var blockResult = _sessionService.Helm.Block();
                helmBlock = blockResult.DamageBlock;
                if (blockResult.IsArmorBrokenDown) _sessionService.RemoveHelm();
            }

            int chestplateBlock = 0;
            if (_sessionService.Chestplate != null)
            {
                var blockResult = _sessionService.Chestplate.Block();
                chestplateBlock = blockResult.DamageBlock;
                if (blockResult.IsArmorBrokenDown) _sessionService.RemoveChestplate();
            }

            int damageAfterBlock = damage - helmBlock - chestplateBlock;
            int playerHealthBeforeAttack = _sessionService.CurrentHealth;
            if (damageAfterBlock > 0) _sessionService.AddCurrentHealth(-damageAfterBlock);
            if (_sessionService.CurrentHealth <= 0) throw new DefeatException($"Вы были повержены {enemy.Name}ОМ.", _gameInfoService.GetGameInfo());
            int enemyHealthAfterAttack = enemyHealthBeforeAttack - enemy.Health;
            return new BattleLog("ИГРОК", damage, playerHealthBeforeAttack, _sessionService.CurrentHealth, enemy.Name!, enemyHealthAfterAttack, enemyHealthBeforeAttack, enemy.Health);
        }
    }
}