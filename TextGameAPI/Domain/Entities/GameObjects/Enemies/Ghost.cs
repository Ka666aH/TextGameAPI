using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Ghost : Enemy
    {
        public Ghost(int id, int roomId)
            : base(id,
                  EnemiesLabeles.GhostName,
                  EnemiesLabeles.GhostDescription,
                  roomId,
                  GameBalance.GhostBaseHealth,
                  GameBalance.GlassSwordBaseDamage,
                  GameBalance.GhostBaseDamageBlock)
        { }
        public override int GetDamage(int damage)
        {
            if (Random.Shared.Next(GameBalance.GhostHitDivider) == 0) Health -= damage;
            return Health;
        }
        private Ghost() { }
    }
}