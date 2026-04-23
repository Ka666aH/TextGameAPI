using TextGame.Domain.Entities.GameObjects.Items;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public abstract class Heal : Item
    {
        public int? MaxHealthBoost { get; protected set; } = 0;
        public int? CurrentHealthBoost { get; protected set; } = 0;

        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public Heal(string name, string description, int id, int roomId, bool fromShop, int? maxHealthBoost, int? currentHealthBoost) : base(name, description, id)
        {
            _roomId = roomId;
            _fromShop = fromShop;

            Initialize(maxHealthBoost, currentHealthBoost);
        }
        protected virtual void Initialize(int? maxHealthBoost, int? currentHealthBoost)
        {
            Cost = 1;
            if (maxHealthBoost is null) MaxHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.CalculateSpread((int)maxHealthBoost!, _roomId);
                MaxHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) GameBalance.CalculateShopMultiplier((int)MaxHealthBoost!);
                Cost += (int)(MaxHealthBoost * GameBalance.MaxHealthCostMultiplier);
            }

            if (currentHealthBoost is null) CurrentHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.CalculateSpread((int)currentHealthBoost!, _roomId);
                CurrentHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) GameBalance.CalculateShopMultiplier((int)CurrentHealthBoost!);
                Cost += (int)(CurrentHealthBoost * GameBalance.CurrentHealthCostMultiplier);
            }
        }
        public virtual (int,int) Use() => ((int)MaxHealthBoost!, (int)CurrentHealthBoost!);
        protected Heal() { }
    }
}