using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public class RegenPotion : Heal
    {
        public RegenPotion(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.RegenPotionName,
                  ItemsLabels.RegenPotionDescription,
                  roomId,
                  fromShop,
                  GameBalance.RegenPotionBaseMaxHealthBoost,
                  GameBalance.RegenPotionBaseCurrentHealthBoost)
        { }
        private RegenPotion() { }
    }
}