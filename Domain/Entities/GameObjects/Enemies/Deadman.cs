using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Deadman : Enemy
    {
        public Deadman(int id, int roomId)
            : base(id,
                  EnemiesLabeles.DeadmanName,
                  EnemiesLabeles.DeadmanDescription,
                  roomId,
                  GameBalance.DeadmanBaseHealth,
                  GameBalance.DeadmanBaseDamage,
                  GameBalance.DeadmanBaseDamageBlock)
        { }
        private Deadman() { }
    }
}