using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items.Equipments;

namespace TextGame.Domain.GameObjects.Items.Equipments.Armors
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
            var (min, max) = GameBalance.ApplySpread(damageBlock, _roomId);
            DamageBlock = Random.Shared.Next(min, max + 1);
            if (_fromShop)
            {
                Durability = GameBalance.ApplyShopMultiplier((int)Durability!);
                DamageBlock = GameBalance.ApplyShopMultiplier(DamageBlock);
            }
            CalculateCost();
        }
        public int Block(IGameSessionService sessionService)
        {
            Durability--;
            CalculateCost();
            if (Durability <= 0) BreakDown(sessionService);
            return DamageBlock;
        }
        protected abstract void BreakDown(IGameSessionService sessionService);
        private void CalculateCost()
        {
            Cost = GameBalance.CalculateArmorCost((int)Durability!, DamageBlock);
        }
    }
}