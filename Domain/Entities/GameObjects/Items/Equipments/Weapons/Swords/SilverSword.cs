using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class SilverSword : Sword
    {
        public SilverSword(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.SilverSwordName,
                  ItemsLabeles.SilverSwordDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.
                  SilverSwordBaseDurability, GameBalance.SilverSwordBaseDamage)
        { }
        private SilverSword() { }
    }
}