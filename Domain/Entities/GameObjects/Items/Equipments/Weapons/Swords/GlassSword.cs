using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords
{
    public class GlassSword : Sword
    {
        public GlassSword(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.GlassSwordName,
                  ItemsLabeles.GlassSwordDescription,
                  roomId,
                  fromShop,
                  GameBalance.GlassSwordBaseDurability,
                  GameBalance.GlassSwordBaseDamage)
        { }
        private GlassSword() { }
    }
}