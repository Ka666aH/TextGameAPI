using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class SkeletorArcher : Enemy
    {
        public SkeletorArcher(int id, int roomId, Random? random = null)
            : base(id,
                  EnemiesLabels.SkeletorArcherName,
                  EnemiesLabels.SkeletorArcherDescription,
                  roomId,
                  GameBalance.SkeletorArcherBaseHealth,
                  GameBalance.SkeletorArcherBaseDamage,
                  GameBalance.SkeletorArcherBaseDamageBlock, random)
        { }
        private SkeletorArcher() { }
    }
}