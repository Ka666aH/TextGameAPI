using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class IronSword : Sword
    {
        public IronSword(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.IronSwordName,
                  ItemsLabels.IronSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronSwordBaseDurability,
                  GameBalance.IronSwordBaseDamage)
        { }
        private IronSword() { }
    }
}