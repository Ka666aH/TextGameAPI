namespace TextGame.Domain.GameObjects.Items.Heal
{
    public class PowerPotion : Heal
    {
        public PowerPotion(int itemId, int roomId, bool fromShop)
            : base("ЗЕЛЬЕ СИЛЫ",
                  "Не только восстанавливает силы, но и придаёт новых.",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.PowerPotionBaseMaxHealthBoost,
                  GameBalance.PowerPotionBaseCurrentHealthBoost)
        { }
    }
}