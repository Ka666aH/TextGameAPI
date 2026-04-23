using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class IronSword : Sword
    {
        public IronSword(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.IronSwordName,
                  ItemsLabeles.IronSwordDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.IronSwordBaseDurability,
                  GameBalance.IronSwordBaseDamage)
        { }
        private IronSword() { }
    }
}