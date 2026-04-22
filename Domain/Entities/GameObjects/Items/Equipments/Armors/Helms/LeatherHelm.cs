using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public class LeatherHelm : Helm
    {
        public LeatherHelm(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.LeatherHelmName,
                  ItemsLabeles.LeatherHelmDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.LeatherHelmBaseDurability,
                  GameBalance.LeatherHelmBaseDamageBlock)
        { }
    }
}