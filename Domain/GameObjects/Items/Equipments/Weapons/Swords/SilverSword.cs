namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Swords
{
    public class SilverSword : Sword
    {
        public SilverSword(int itemId, int roomId, bool fromShop)
            : base("СЕРЕБРЯНЫЙ МЕЧ",
                  "Редкий меч из особого серебряного сплава. Эффективный, но менее прочный.",
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.
                  SilverSwordBaseDurability, GameBalance.SilverSwordBaseDamage)
        { }
    }
}