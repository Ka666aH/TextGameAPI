namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public abstract class Heal : Item
    {
        public int? MaxHealthBoost { get; protected set; } = 0;
        public int? CurrentHealthBoost { get; protected set; } = 0;

        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public Heal(int id, string name, string description, int roomId, bool fromShop, int? maxHealthBoost, int? currentHealthBoost)
            : base(id, name, description, roomId)
        {
            _roomId = roomId;
            _fromShop = fromShop;

            Initialize(maxHealthBoost, currentHealthBoost);
        }
        protected virtual void Initialize(int? maxHealthBoost, int? currentHealthBoost)
        {
            if (maxHealthBoost is null) MaxHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.CalculateSpread((int)maxHealthBoost!, _roomId);
                MaxHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) MaxHealthBoost = GameBalance.CalculateShopMultiplier((int)MaxHealthBoost!);
            }

            if (currentHealthBoost is null) CurrentHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.CalculateSpread((int)currentHealthBoost!, _roomId);
                CurrentHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) CurrentHealthBoost = GameBalance.CalculateShopMultiplier((int)CurrentHealthBoost!);
            }
            Cost = GameBalance.CalculateHealCost(MaxHealthBoost, CurrentHealthBoost);
        }
        public virtual (int, int) Use() => ((int)MaxHealthBoost!, (int)CurrentHealthBoost!);
        protected Heal() { }
    }
}