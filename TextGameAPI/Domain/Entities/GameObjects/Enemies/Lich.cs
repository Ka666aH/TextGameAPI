using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Lich : Enemy
    {
        public Lich(int id, int roomId, Random? random = null)
            : base(id,
                  EnemiesLabels.LichName,
                  EnemiesLabels.LichDescription,
                  roomId,
                  GameBalance.LichBaseHealth,
                  GameBalance.LichBaseDamage,
                  GameBalance.LichBaseDamageBlock, random)
        { }
        private Lich() { }
    }
}