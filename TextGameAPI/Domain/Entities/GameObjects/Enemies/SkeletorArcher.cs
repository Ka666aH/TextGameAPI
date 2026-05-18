using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class SkeletorArcher : Enemy
    {
        public SkeletorArcher(int id, int roomId)
            : base(id,
                  EnemiesLabeles.SkeletorArcherName,
                  EnemiesLabeles.SkeletorArcherDescription,
                  roomId,
                  GameBalance.SkeletorArcherBaseHealth,
                  GameBalance.SkeletorArcherBaseDamage,
                  GameBalance.SkeletorArcherBaseDamageBlock)
        { }
        private SkeletorArcher() { }
    }
}