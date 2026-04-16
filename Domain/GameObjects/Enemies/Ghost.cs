using TextGame.Domain.GameObjects.Rooms;
using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Enemies
{
    public class Ghost : Enemy
    {
        public Ghost(int roomId, int enemyId)
            : base(EnemiesLabeles.GhostName,
                  EnemiesLabeles.GhostDescription,
                  roomId,
                  enemyId,
                  GameBalance.GhostBaseHealth,
                  GameBalance.GlassSwordBaseDamage,
                  GameBalance.GhostBaseDamageBlock)
        { }
        public override int GetDamage(int damage)
        {
            if (Random.Shared.Next(GameBalance.GhostHitDivider) == 0) Health -= damage;
            return Health;
        }
    }
}