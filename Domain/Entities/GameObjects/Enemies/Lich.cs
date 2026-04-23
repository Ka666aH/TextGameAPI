using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public class Lich : Enemy
    {
        public Lich(int id,int roomId)
            : base(id,
                  EnemiesLabeles.LichName,
                  EnemiesLabeles.LichDescription,
                  roomId,
                  GameBalance.LichBaseHealth,
                  GameBalance.LichBaseDamage,
                  GameBalance.LichBaseDamageBlock)
        { }
        private Lich() { }
    }
}