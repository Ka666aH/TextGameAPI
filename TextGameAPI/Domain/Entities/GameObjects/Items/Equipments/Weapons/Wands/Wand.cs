using TextGame.Domain.DTO;
namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands
{
    public abstract class Wand : Weapon
    {
        public Wand(int id, string name, string description, int roomId, bool fromShop, int damage, Random? random = null)
            : base(id, name, description, null, 0, roomId, fromShop)
        {
            Initialize(damage, random);
        }
        protected void Initialize(int damage, Random? random = null)
        {
            var rng = random ?? Random.Shared;
            var (min, max) = GameBalance.CalculateSpread(damage, _roomId);
            Damage = rng.Next(min, max + 1);
            if (_fromShop) Damage = GameBalance.CalculateShopMultiplier(Damage);
            Cost = GameBalance.CalculateWandCost(Damage);
        }
        public override WeaponAttackResult Attack(int roomId) => new WeaponAttackResult(Damage);
        protected Wand() { }
    }
}