using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Heal
{
    public class Bandage : Heal
    {
        public Bandage(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.BandageName,
                  ItemsLabeles.BandageDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.BandageBaseMaxHealthBoost,
                  GameBalance.BandageBaseCurrentHealthBoost)
        { }
    }
}