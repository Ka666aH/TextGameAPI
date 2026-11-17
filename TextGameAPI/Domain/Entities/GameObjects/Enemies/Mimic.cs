using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Mimic : Enemy
    {
        public Mimic(int id, int roomId, Random? random = null)
            : base(id,
                  EnemiesLabels.MimicName,
                  EnemiesLabels.MimicDescription,
                  roomId,
                  GameBalance.MimicBaseHealth,
                  GameBalance.MimicBaseDamage,
                  GameBalance.MimicBaseDamageBlock, random)
        { }
        private Mimic() { }
    }
}
