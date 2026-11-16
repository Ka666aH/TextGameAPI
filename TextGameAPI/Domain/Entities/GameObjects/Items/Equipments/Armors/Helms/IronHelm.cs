using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class IronHelm : Helm
    {
        public IronHelm(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.IronHelmName,
                  ItemsLabels.IronHelmDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronHelmBaseDurability,
                  GameBalance.IronHelmBaseDamageBlock, random)
        { }
        private IronHelm() { }
    }
}