using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public class RandomPotion : Heal
    {
        public RandomPotion(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.RandomPotionName,
                  ItemsLabels.RandomPotionDescription,
                  roomId,
                  fromShop,
                  null,
                  null)
        {
            var (minCost, maxCost) = GameBalance.CalculateSpread(GameBalance.CalculateRandomPotionBaseCost(), roomId);
            Cost = Random.Shared.Next(minCost, maxCost);
            if (fromShop) Cost = (int)(Cost! * GameBalance.StoreMargin);
        }
        public override (int, int) Use()
        {
            double gain = GameBalance.CalculateGain(_roomId);
            int maxBase = (int)(GameBalance.RandomPotionBaseMaxHealthBoost * gain);
            int currentBase = (int)(GameBalance.RandomPotionBaseCurrentHealthBoost * gain);
            if (_fromShop)
            {
                maxBase = GameBalance.CalculateShopMultiplier(maxBase);
                currentBase = GameBalance.CalculateShopMultiplier(currentBase);
            }
            int maxRoll = Random.Shared.Next(-maxBase, maxBase * 2 + 1);
            int currentRoll = Random.Shared.Next(-currentBase, currentBase * 2 + 1);
            MaxHealthBoost = maxRoll;
            CurrentHealthBoost = currentRoll;
            return base.Use();
        }
        private RandomPotion() { }
    }
}
