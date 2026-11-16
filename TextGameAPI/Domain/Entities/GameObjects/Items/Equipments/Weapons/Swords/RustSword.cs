using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class RustSword : Sword
    {
        public RustSword(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.RustSwordName,
                  ItemsLabels.RustSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.RustSwordBaseDurability,
                  GameBalance.RustSwordBaseDamage, random)
        { }
        private RustSword() { }
    }
}