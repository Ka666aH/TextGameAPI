namespace TextGame.Domain.GameObjects.Items.Heal
{
    public class RegenPotion : Heal
    {
        public RegenPotion(int itemId, int roomId, bool fromShop)
            : base("ЗЕЛЬЕ РЕГЕНЕРАЦИИ",
                  "Пыльный бутылёк с субстанцией тёмного цвета.",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.RegenPotionBaseMaxHealthBoost,
                  GameBalance.RegenPotionBaseCurrentHealthBoost)
        { }
    }
}