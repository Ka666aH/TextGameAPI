using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class RustSword : Sword
    {
        public RustSword(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.RustSwordName,
                  ItemsLabeles.RustSwordDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.RustSwordBaseDurability,
                  GameBalance.RustSwordBaseDamage)
        { }
    }
}