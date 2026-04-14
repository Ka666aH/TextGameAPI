namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Wands
{
    public class RandomWand : Wand
    {
        public RandomWand(int itemId, int roomId, bool fromShop)
            : base("ЖЕЗЛ СЛУЧАЙНОСТЕЙ", "Странное магическое оружие. Становится сильнее со временем.", itemId, roomId, fromShop, GameBalance.RandomWandBaseDamage) { }
        public override WeaponAttackResult Attack(int roomId)
        {
            int damage = (int)(Damage * GameBalance.CalculateGain(_roomId));
            return new WeaponAttackResult(Random.Shared.Next(damage + 1));
        }
    }
}