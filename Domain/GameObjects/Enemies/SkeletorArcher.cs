namespace TextGame.Domain.GameObjects.Enemies
{
    public class SkeletorArcher : Enemy
    {
        public SkeletorArcher(int roomId, int enemyId)
            : base("СКЕЛЕТОР-ЛУЧНИК",
                  "Из тех, кто при жизни умел обращаться с луком.",
                  roomId,
                  enemyId,
                  GameBalance.SkeletorArcherBaseHealth,
                  GameBalance.SkeletorArcherBaseDamage,
                  GameBalance.SkeletorArcherBaseDamageBlock)
        { }
    }
}