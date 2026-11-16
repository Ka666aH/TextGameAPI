using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class GlassSword : Sword
    {
        public GlassSword(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.GlassSwordName,
                  ItemsLabels.GlassSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.GlassSwordBaseDurability,
                  GameBalance.GlassSwordBaseDamage, random)
        { }
        private GlassSword() { }
    }
}