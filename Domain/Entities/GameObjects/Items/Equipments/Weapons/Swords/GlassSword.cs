using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class GlassSword : Sword
    {
        public GlassSword(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.GlassSwordName,
                  ItemsLabeles.GlassSwordDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.GlassSwordBaseDurability,
                  GameBalance.GlassSwordBaseDamage)
        { }
        private GlassSword() { }
    }
}