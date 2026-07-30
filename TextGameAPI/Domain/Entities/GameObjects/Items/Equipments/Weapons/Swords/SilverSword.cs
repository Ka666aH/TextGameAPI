using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class SilverSword : Sword
    {
        public SilverSword(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.SilverSwordName,
                  ItemsLabels.SilverSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.
                  SilverSwordBaseDurability, GameBalance.SilverSwordBaseDamage)
        { }
        private SilverSword() { }
    }
}