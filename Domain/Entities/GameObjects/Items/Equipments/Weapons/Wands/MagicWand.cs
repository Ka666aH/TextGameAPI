using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands
{
    public class MagicWand : Wand
    {

        public MagicWand(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.MagicWandName,
                  ItemsLabeles.MagicWandDescription,
                  roomId,
                  fromShop,
                  GameBalance.MagicWandBaseDamage)
        { }
        private MagicWand() { }
    }
}