namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Swords
{
    public class RustSword : Sword
    {
        public RustSword(int itemId, int roomId, bool fromShop)
            : base("РЖАВЫЙ МЕЧ",
                  "Очень старый меч. Лучше, чем ничего.",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.RustSwordBaseDurability,
                  GameBalance.RustSwordBaseDamage)
        { }
    }
}