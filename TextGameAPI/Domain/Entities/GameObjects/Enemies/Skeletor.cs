using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Skeletor : Enemy
    {

        public Skeletor(int id, int roomId, Random? random = null)
            : base(id,
                  EnemiesLabels.SkeletorName,
                  EnemiesLabels.SkeletorDescription,
                  roomId,
                  GameBalance.SkeletorBaseHealth,
                  GameBalance.SkeletorBaseDamage,
                  GameBalance.SkeletorBaseDamageBlock, random)
        { }
        private Skeletor() { }
    }
}