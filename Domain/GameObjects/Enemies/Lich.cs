namespace TextGame.Domain.GameObjects.Enemies
{
    public class Lich : Enemy
    {
        public Lich(int roomId, int enemyId)
            : base("ЛИЧ", "Тебя ждёт вечный параЛИЧ. Ха-ха.", roomId, enemyId, GameBalance.LichBaseHealth, GameBalance.LichBaseDamage, GameBalance.LichBaseDamageBlock) { }
    }
}