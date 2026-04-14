namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms
{
    public class IronHelm : Helm
    {
        public IronHelm(int itemId, int roomId, bool fromShop)
            : base("ЖЕЛЕЗНЫЙ ШЛЕМ", "Крепкий шлем из качественного металла.", itemId, roomId, fromShop, GameBalance.IronHelmBaseDurability, GameBalance.IronHelmBaseDamageBlock) { }
    }
}