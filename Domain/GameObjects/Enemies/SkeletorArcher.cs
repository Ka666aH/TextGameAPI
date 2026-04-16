using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Enemies
{
    public class SkeletorArcher : Enemy
    {
        public SkeletorArcher(int roomId, int enemyId)
            : base(EnemiesLabeles.SkeletorArcherName,
                  EnemiesLabeles.SkeletorArcherDescription,
                  roomId,
                  enemyId,
                  GameBalance.SkeletorArcherBaseHealth,
                  GameBalance.SkeletorArcherBaseDamage,
                  GameBalance.SkeletorArcherBaseDamageBlock)
        { }
    }
}