using TextGame.Domain.DTO;

namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Wands
{
    public abstract class Wand : Weapon
    {
        public Wand(string name, string description, int itemId, int roomId, bool fromShop, int damage) : base(name, description, itemId, null, 0, roomId, fromShop)
        {
            Initialize(damage);
        }
        protected void Initialize(int damage)
        {
            var (min, max) = GameBalance.CalculateSpread(damage, _roomId);
            Damage = Random.Shared.Next(min, max + 1);
            if (_fromShop) Damage = GameBalance.CalculateShopMultiplier(Damage);
            Cost = GameBalance.CalculateWandCost(Damage);
        }
        public override WeaponAttackResult Attack(int roomId) => new WeaponAttackResult(Damage);
    }
}