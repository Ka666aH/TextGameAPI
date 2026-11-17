using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Deadman : Enemy
    {
        public Deadman(int id, int roomId, Random? random = null)
            : base(id,
                  EnemiesLabels.DeadmanName,
                  EnemiesLabels.DeadmanDescription,
                  roomId,
                  GameBalance.DeadmanBaseHealth,
                  GameBalance.DeadmanBaseDamage,
                  GameBalance.DeadmanBaseDamageBlock, random)
        { }
        private Deadman() { }
    }
}