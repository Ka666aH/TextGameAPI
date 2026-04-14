namespace TextGame.Domain.GameObjects.Enemies
{
    public class Deadman : Enemy
    {
        public Deadman(int roomId, int enemyId)
            : base("МЕРТВЯК",
                  "Мёртвое полуразложившееся тело. Источник жуткого смрада.",
                  roomId,
                  enemyId,
                  GameBalance.DeadmanBaseHealth,
                  GameBalance.DeadmanBaseDamage,
                  GameBalance.DeadmanBaseDamageBlock)
        { }
    }
}