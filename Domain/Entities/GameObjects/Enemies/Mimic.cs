using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Mimic : Enemy
    {
        public Mimic(int roomId, int enemyId)
            : base(EnemiesLabeles.MimicName,
                  EnemiesLabeles.MimicDescription,
                  roomId,
                  enemyId,
                  GameBalance.MimicBaseHealth,
                  GameBalance.MimicBaseDamage,
                  GameBalance.MimicBaseDamageBlock)
        { }
    }
}
