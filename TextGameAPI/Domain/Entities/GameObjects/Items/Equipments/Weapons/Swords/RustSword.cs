using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class RustSword : Sword
    {
        public RustSword(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.RustSwordName,
                  ItemsLabels.RustSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.RustSwordBaseDurability,
                  GameBalance.RustSwordBaseDamage)
        { }
        private RustSword() { }
    }
}