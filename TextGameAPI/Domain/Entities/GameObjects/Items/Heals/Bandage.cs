using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Heals
{
    public class Bandage : Heal
    {
        public Bandage(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.BandageName,
                  ItemsLabels.BandageDescription,
                  roomId,
                  fromShop,
                  GameBalance.BandageBaseMaxHealthBoost,
                  GameBalance.BandageBaseCurrentHealthBoost)
        { }
        private Bandage() { }
    }
}