using TextGame.Application.Interfaces.Services;

namespace TextGame.Domain.GameObjects.Items.Heal
{
    public abstract class Heal : Item
    {
        public int? MaxHealthBoost { get; protected set; } = 0;
        public int? CurrentHealthBoost { get; protected set; } = 0;

        protected readonly int _roomId;
        protected readonly bool _fromShop;

        public Heal(string name, string description, int id, int roomId, bool fromShop, int? maxHealthBoost, int? currentHealthBoost) : base(name, description, id, true)
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
                var (min, max) = GameBalance.ApplySpread((int)maxHealthBoost!, _roomId);
                MaxHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) GameBalance.ApplyShopMultiplier((int)MaxHealthBoost!);
                Cost += (int)(MaxHealthBoost * GameBalance.MaxHealthCostMultiplier);
            }

            if (currentHealthBoost is null) CurrentHealthBoost = null;
            else
            {
                var (min, max) = GameBalance.ApplySpread((int)currentHealthBoost!, _roomId);
                CurrentHealthBoost = Random.Shared.Next(min, max + 1);
                if (_fromShop) GameBalance.ApplyShopMultiplier((int)CurrentHealthBoost!);
                Cost += (int)(CurrentHealthBoost * GameBalance.CurrentHealthCostMultiplier);
            }
        }
        public virtual void Use(IGameSessionService sessionService)
        {
            sessionService.AddMaxHealth((int)MaxHealthBoost!);
            sessionService.AddCurrentHealth((int)CurrentHealthBoost!);
        }
    }
}