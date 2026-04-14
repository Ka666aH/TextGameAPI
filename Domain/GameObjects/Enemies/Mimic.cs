namespace TextGame.Domain.GameObjects.Enemies
{
    public class Mimic : Enemy
    {
        public Mimic(int roomId, int enemyId)
            : base("МИМИК",
                  "Подлый монстр, изменяющий свой облик для охоты на неосторожных попаданцев.",
                  roomId,
                  enemyId,
                  GameBalance.MimicBaseHealth,
                  GameBalance.MimicBaseDamage,
                  GameBalance.MimicBaseDamageBlock)
        { }
    }
}
