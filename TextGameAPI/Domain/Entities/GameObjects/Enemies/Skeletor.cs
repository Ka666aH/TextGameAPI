using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Skeletor : Enemy
    {

        public Skeletor(int id, int roomId)
            : base(id,
                  EnemiesLabels.SkeletorName,
                  EnemiesLabels.SkeletorDescription,
                  roomId,
                  GameBalance.SkeletorBaseHealth,
                  GameBalance.SkeletorBaseDamage,
                  GameBalance.SkeletorBaseDamageBlock)
        { }
        private Skeletor() { }
    }
}