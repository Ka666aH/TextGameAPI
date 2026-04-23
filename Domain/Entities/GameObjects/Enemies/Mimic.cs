using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Mimic : Enemy
    {
        public Mimic(int id, int roomId)
            : base(id,
                  EnemiesLabeles.MimicName,
                  EnemiesLabeles.MimicDescription,
                  roomId,
                  GameBalance.MimicBaseHealth,
                  GameBalance.MimicBaseDamage,
                  GameBalance.MimicBaseDamageBlock)
        { }
        private Mimic() { }
    }
}
