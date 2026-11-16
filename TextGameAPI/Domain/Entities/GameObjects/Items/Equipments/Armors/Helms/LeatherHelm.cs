using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class LeatherHelm : Helm
    {
        public LeatherHelm(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.LeatherHelmName,
                  ItemsLabels.LeatherHelmDescription,
                  roomId,
                  fromShop,
                  GameBalance.LeatherHelmBaseDurability,
                  GameBalance.LeatherHelmBaseDamageBlock, random)
        { }
        private LeatherHelm() { }
    }
}