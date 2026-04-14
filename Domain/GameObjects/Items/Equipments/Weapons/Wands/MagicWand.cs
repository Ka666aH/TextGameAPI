namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Wands
{
    public class MagicWand : Wand
    {

        public MagicWand(int itemId, int roomId, bool fromShop)
            : base("ВОЛШЕБНЫЙ ЖЕЗЛ", "Простое магическое оружие. Может использовать каждый.", itemId, roomId, fromShop, GameBalance.MagicWandBaseDamage) { }
    }
}