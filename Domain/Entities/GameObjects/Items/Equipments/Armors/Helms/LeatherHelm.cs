using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class LeatherHelm : Helm
    {
        public LeatherHelm(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.LeatherHelmName,
                  ItemsLabeles.LeatherHelmDescription,
                  roomId,
                  fromShop,
                  GameBalance.LeatherHelmBaseDurability,
                  GameBalance.LeatherHelmBaseDamageBlock)
        { }
        private LeatherHelm() { }
    }
}