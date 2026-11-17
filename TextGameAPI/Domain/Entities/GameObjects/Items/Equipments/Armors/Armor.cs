using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors
{
    public abstract class Armor : Equipment
    {
        public int DamageBlock { get; protected set; }
        public Armor(int id, string name, string description, int roomId, bool fromShop, int durability, int damageBlock, Random? random = null)
            : base(id, name, description, durability, roomId, fromShop)
        {
            Initialize(durability, damageBlock, random);
        }
        protected void Initialize(int durability, int damageBlock, Random? random = null)
        {
            var rng = random ?? Random.Shared;
            Durability = rng.Next(
                (int)(durability * GameBalance.SpreadFloor),
                (int)(durability * GameBalance.SpreadCeiling + 1));
            var (min, max) = GameBalance.CalculateSpread(damageBlock, _roomId);
            DamageBlock = rng.Next(min, max + 1);
            if (_fromShop)
            {
                Durability = GameBalance.CalculateShopMultiplier((int)Durability!);
                DamageBlock = GameBalance.CalculateShopMultiplier(DamageBlock);
            }
            CalculateCost();
        }
        public ArmorBlockResult Block()
        {
            Durability--;
            CalculateCost();
            return new(DamageBlock, Durability <= 0);
        }
        private void CalculateCost()
        {
            Cost = GameBalance.CalculateArmorCost((int)Durability!, DamageBlock);
        }
        protected Armor() { }
    }
}