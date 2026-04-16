using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Enemies
{
    public class Deadman : Enemy
    {
        public Deadman(int roomId, int enemyId)
            : base(EnemiesLabeles.DeadmanName,
                  EnemiesLabeles.DeadmanDescription,
                  roomId,
                  enemyId,
                  GameBalance.DeadmanBaseHealth,
                  GameBalance.DeadmanBaseDamage,
                  GameBalance.DeadmanBaseDamageBlock)
        { }
    }
}