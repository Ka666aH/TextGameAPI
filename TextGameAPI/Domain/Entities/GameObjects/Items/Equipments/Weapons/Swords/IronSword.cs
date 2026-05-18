using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class IronSword : Sword
    {
        public IronSword(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.IronSwordName,
                  ItemsLabeles.IronSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronSwordBaseDurability,
                  GameBalance.IronSwordBaseDamage)
        { }
        private IronSword() { }
    }
}