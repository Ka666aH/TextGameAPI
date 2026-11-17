using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public abstract class Sword : Weapon
    {
        public Sword(int id, string name, string description, int roomId, bool fromShop, int durability, int damage, Random? random = null)
            : base(id, name, description, null, 0, roomId, fromShop)
        {
            Initialize(durability, damage, random);
        }
        protected void Initialize(int durability, int damage, Random? random = null)
        {
            var rng = random ?? Random.Shared;
            var (minDurability, maxDurability) = GameBalance.CalculateSpread(durability, _roomId);
            Durability = Math.Max(1, rng.Next(minDurability, maxDurability + 1));
            var (minDamage, maxDamage) = GameBalance.CalculateSpread(damage, _roomId);
            Damage = rng.Next(minDamage, maxDamage + 1);
            if (_fromShop)
            {
                Durability = GameBalance.CalculateShopMultiplier((int)Durability!);
                Damage = GameBalance.CalculateShopMultiplier(Damage);
            }
            CalculateCost();
        }
        public override WeaponAttackResult Attack(int roomId)
        {
            Durability--;
            CalculateCost();
            return new WeaponAttackResult(Damage, IsWeaponBrokenDown: Durability <= 0);
        }
        private void CalculateCost()
        {
            Cost = GameBalance.CalculateSwordCost((int)Durability!, Damage);
        }
        protected Sword() { }
    }
}