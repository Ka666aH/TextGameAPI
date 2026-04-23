using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public abstract class Sword : Weapon
    {
        public Sword(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damage) : base(name, description, itemId, null, 0, roomId, fromShop)
        {
            Initialize(durability, damage);
        }
        protected void Initialize(int durability, int damage)
        {
            var (minDurability, maxDurability) = GameBalance.CalculateSpread(durability, _roomId);
            Durability = Random.Shared.Next(minDurability, maxDurability + 1);
            var (minDamage, maxDamage) = GameBalance.CalculateSpread(damage, _roomId);
            Damage = Random.Shared.Next(minDamage, maxDamage + 1);
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