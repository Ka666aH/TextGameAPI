using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands
{
    public class MagicWand : Wand
    {

        public MagicWand(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.MagicWandName,
                  ItemsLabels.MagicWandDescription,
                  roomId,
                  fromShop,
                  GameBalance.MagicWandBaseDamage, random)
        { }
        private MagicWand() { }
    }
}