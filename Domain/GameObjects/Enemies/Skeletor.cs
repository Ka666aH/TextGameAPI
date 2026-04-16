using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Enemies
{
    public class Skeletor : Enemy
    {

        public Skeletor(int roomId, int enemyId)
            : base(EnemiesLabeles.SkeletorName,
                  EnemiesLabeles.SkeletorDescription,
                  roomId,
                  enemyId,
                  GameBalance.SkeletorBaseHealth,
                  GameBalance.SkeletorBaseDamage,
                  GameBalance.SkeletorBaseDamageBlock)
        { }
    }
}