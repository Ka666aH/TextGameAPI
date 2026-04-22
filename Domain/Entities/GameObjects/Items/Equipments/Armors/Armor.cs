using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors
{
    public abstract class Armor : Equipment
    {
        public int DamageBlock { get; protected set; }
        public Armor(string name, string description, int id, int roomId, bool fromShop, int durability, int damageBlock) : base(name, description, id, durability, roomId, fromShop)
        {
            Initialize(durability, damageBlock);
        }
        protected void Initialize(int durability, int damageBlock)
        {
            Durability = Random.Shared.Next(
                (int)(durability * GameBalance.SpreadFloor),
                (int)(durability * GameBalance.SpreadCeiling + 1));
            var (min, max) = GameBalance.CalculateSpread(damageBlock, _roomId);
            DamageBlock = Random.Shared.Next(min, max + 1);
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
    }
}