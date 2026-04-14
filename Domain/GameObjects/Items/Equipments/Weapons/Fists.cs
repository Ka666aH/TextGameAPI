namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons
{
    public class Fists : Weapon
    {
        public static readonly Fists DefaultFists = new Fists();

        public Fists() : base("КУЛАКИ", "То, что есть (почти) у каждого. Базовое оружие самозащиты. Может быть больно.", null, null, GameBalance.FistsBaseDamage, 0, false) { }
        public override WeaponAttackResult Attack(int roomId)
        {
            var (min, max) = GameBalance.CalculateSpread(GameBalance.FistsBaseDamage, roomId);
            int damage = Random.Shared.Next(min, max + 1);
            int selfDamage =
                Random.Shared.Next(GameBalance.FistSelfHarmProbabilityDivider) == 0 ?
                ((int)(damage / GameBalance.FistSelfHarmDivider)) :
                0;

            return new WeaponAttackResult(damage, selfDamage);
        }
    }
}