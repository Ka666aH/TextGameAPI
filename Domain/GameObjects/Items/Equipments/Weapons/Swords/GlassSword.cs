namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Swords
{
    public class GlassSword : Sword
    {
        public GlassSword(int itemId, int roomId, bool fromShop)
            : base("СТЕКЛЯННЫЙ МЕЧ",
                  "Скорее объект искусства, чем оружие. Очень хрупкий, но невероятно сильный.",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.GlassSwordBaseDurability,
                  GameBalance.GlassSwordBaseDamage)
        { }
    }
}