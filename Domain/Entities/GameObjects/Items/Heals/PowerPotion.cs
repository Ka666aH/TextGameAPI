using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
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
        private PowerPotion() { }
    }
}