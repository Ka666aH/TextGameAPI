using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Wands
{
    public class MagicWand : Wand
    {

        public MagicWand(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.MagicWandName,
                  ItemsLabeles.MagicWandDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.MagicWandBaseDamage)
        { }
    }
}