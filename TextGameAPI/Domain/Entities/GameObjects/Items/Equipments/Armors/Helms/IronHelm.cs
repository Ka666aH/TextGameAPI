using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class IronHelm : Helm
    {
        public IronHelm(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.IronHelmName,
                  ItemsLabeles.IronHelmDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronHelmBaseDurability,
                  GameBalance.IronHelmBaseDamageBlock)
        { }
        private IronHelm() { }
    }
}