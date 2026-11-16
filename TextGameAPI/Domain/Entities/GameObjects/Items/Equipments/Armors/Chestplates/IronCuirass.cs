using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class IronCuirass : Chestplate
    {
        public IronCuirass(int id, int roomId, bool fromShop, Random? random = null)
            : base(id,
                  ItemsLabels.IronCuirassName,
                  ItemsLabels.IronCuirassDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronCuirassBaseDurability,
                  GameBalance.IronCuirassBaseDamageBlock, random)
        { }
        private IronCuirass() { }
    }
}