namespace TextGame.Domain.GameObjects.Enemies
{
    public class Skeletor : Enemy
    {

        public Skeletor(int roomId, int enemyId)
            : base("СКЕЛЕТОР", "Чей-то скелет, наделёный возможностью двигаться.", roomId, enemyId, GameBalance.SkeletorBaseHealth, GameBalance.SkeletorBaseDamage, GameBalance.SkeletorBaseDamageBlock) { }
    }
}