using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public class RegenPotion : Heal
    {
        public RegenPotion(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.RegenPotionName,
                  ItemsLabeles.RegenPotionDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.RegenPotionBaseMaxHealthBoost,
                  GameBalance.RegenPotionBaseCurrentHealthBoost)
        { }
    }
}