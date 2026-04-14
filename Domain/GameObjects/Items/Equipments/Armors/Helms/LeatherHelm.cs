namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms
{
    public class LeatherHelm : Helm
    {
        public LeatherHelm(int itemId, int roomId, bool fromShop)
            : base("КОЖАННЫЙ ШЛЕМ", "Изысканный чёрный шлем мастера подземелия.", itemId, roomId, fromShop, GameBalance.LeatherHelmBaseDurability, GameBalance.LeatherHelmBaseDamageBlock) { }
    }
}