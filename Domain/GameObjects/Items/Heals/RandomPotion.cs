using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Heal
{
    public class RandomPotion : Heal
    {
        public RandomPotion(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.RandomPotionName,
                  ItemsLabeles.RandomPotionDescription,
                  itemId,
                  roomId,
                  fromShop,
                  null,
                  null)
        { }
        public override (int, int) Use()
        {
            double maxHealthFloor = GameBalance.RandomPotionBaseMaxHealthBoost * GameBalance.CalculateGain(_roomId) * GameBalance.SpreadFloor;
            double maxHealthCeiling = GameBalance.RandomPotionBaseMaxHealthBoost * GameBalance.CalculateGain(_roomId) * GameBalance.SpreadCeiling;
            double currentHealthFloor = GameBalance.RandomPotionBaseCurrentHealthBoost * GameBalance.CalculateGain(_roomId) * GameBalance.SpreadFloor;
            double currentHealthCeiling = GameBalance.RandomPotionBaseCurrentHealthBoost * GameBalance.CalculateGain(_roomId) * GameBalance.SpreadCeiling;
            if (_fromShop)
            {
                maxHealthFloor *= 1 / GameBalance.ShopMultiplier;
                maxHealthCeiling *= GameBalance.ShopMultiplier;
                currentHealthFloor *= 1 / GameBalance.ShopMultiplier;
                currentHealthCeiling *= GameBalance.ShopMultiplier;
            }
            MaxHealthBoost = Random.Shared.Next((int)maxHealthFloor, (int)currentHealthCeiling + 1);
            CurrentHealthBoost = Random.Shared.Next((int)currentHealthFloor, (int)currentHealthCeiling + 1);
            return base.Use();
        }
    }
}