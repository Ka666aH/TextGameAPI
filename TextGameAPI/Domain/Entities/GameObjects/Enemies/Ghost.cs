using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Ghost : Enemy
    {
        public Ghost(int id, int roomId)
            : base(id,
                  EnemiesLabels.GhostName,
                  EnemiesLabels.GhostDescription,
                  roomId,
                  GameBalance.GhostBaseHealth,
                  GameBalance.GhostBaseDamage,
                  GameBalance.GhostBaseDamageBlock)
        { }
        public override void GetDamage(int damage)
        {
            if (Random.Shared.Next(GameBalance.GhostHitDivider) == 0) Health -= damage;
        }
        private Ghost() { }
    }
}