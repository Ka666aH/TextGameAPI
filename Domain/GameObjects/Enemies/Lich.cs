using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Enemies
{
    public class Lich : Enemy
    {
        public Lich(int roomId, int enemyId)
            : base(EnemiesLabeles.LichName,
                  EnemiesLabeles.LichDescription,
                  roomId,
                  enemyId,
                  GameBalance.LichBaseHealth,
                  GameBalance.LichBaseDamage,
                  GameBalance.LichBaseDamageBlock)
        { }
    }
}