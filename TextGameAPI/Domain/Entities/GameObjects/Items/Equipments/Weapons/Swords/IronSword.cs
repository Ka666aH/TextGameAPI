using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class IronSword : Sword
    {
        public IronSword(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.IronSwordName,
                  ItemsLabels.IronSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronSwordBaseDurability,
                  GameBalance.IronSwordBaseDamage, random)
        { }
        private IronSword() { }
    }
}