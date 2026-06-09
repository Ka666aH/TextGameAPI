using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public class PowerPotion : Heal
    {
        public PowerPotion(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.PowerPotionName,
                  ItemsLabels.PowerPotionDescription,
                  roomId,
                  fromShop,
                  GameBalance.PowerPotionBaseMaxHealthBoost,
                  GameBalance.PowerPotionBaseCurrentHealthBoost)
        { }
        private PowerPotion() { }
    }
}