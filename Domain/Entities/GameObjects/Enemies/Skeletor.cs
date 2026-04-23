using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Skeletor : Enemy
    {

        public Skeletor(int id, int roomId)
            : base(id,
                  EnemiesLabeles.SkeletorName,
                  EnemiesLabeles.SkeletorDescription,
                  roomId,
                  GameBalance.SkeletorBaseHealth,
                  GameBalance.SkeletorBaseDamage,
                  GameBalance.SkeletorBaseDamageBlock)
        { }
        private Skeletor() { }
    }
}