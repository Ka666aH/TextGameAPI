using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms
{
    public class IronHelm : Helm
    {
        public IronHelm(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.IronHelmName,
                  ItemsLabeles.IronHelmDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.IronHelmBaseDurability,
                  GameBalance.IronHelmBaseDamageBlock)
        { }
    }
}