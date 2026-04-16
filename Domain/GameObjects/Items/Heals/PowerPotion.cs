using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Heal
{
    public class PowerPotion : Heal
    {
        public PowerPotion(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.PowerPotionName,
                  ItemsLabeles.PowerPotionDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.PowerPotionBaseMaxHealthBoost,
                  GameBalance.PowerPotionBaseCurrentHealthBoost)
        { }
    }
}