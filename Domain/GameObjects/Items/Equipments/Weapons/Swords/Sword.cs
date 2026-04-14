using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;

namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Swords
{
    public abstract class Sword : Weapon
    {
        public Sword(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damage) : base(name, description, itemId, null, 0, roomId, fromShop)
        {
            Initialize(durability, damage);
        }
        protected void Initialize(int durability, int damage)
        {
            var (minDurability, maxDurability) = GameBalance.ApplySpread(durability, _roomId);
            Durability = Random.Shared.Next(minDurability, maxDurability + 1);
            var (minDamage, maxDamage) = GameBalance.ApplySpread(damage, _roomId);
            Damage = Random.Shared.Next(minDamage, maxDamage + 1);
            if (_fromShop)
            {
                Durability = GameBalance.ApplyShopMultiplier((int)Durability!);
                Damage = GameBalance.ApplyShopMultiplier(Damage);
            }
            CalculateCost();
        }
        public override int Attack(IGameSessionService sessionService)
        {
            Durability--;
            CalculateCost();
            if (Durability <= 0) BreakDown(sessionService);
            return Damage;
        }
        public void BreakDown(IGameSessionService sessionService)
        {
            sessionService.RemoveWeapon();
        }
        private void CalculateCost()
        {
            Cost = GameBalance.CalculateSwordCost((int)Durability!, Damage);
        }
    }
}