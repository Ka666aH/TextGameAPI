namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons.Swords
{
    public class IronSword : Sword
    {
        public IronSword(int itemId, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНЫЙ МЕЧ", "Добротное оружие воина. На лезвии оттиск \"304\".Номер, наверное.", itemId, roomId, fromShop, GameBalance.IronSwordBaseDurability, GameBalance.IronSwordBaseDamage) { }
    }
}