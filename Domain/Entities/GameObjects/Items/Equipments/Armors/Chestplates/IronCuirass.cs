using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class IronCuirass : Chestplate
    {
        public IronCuirass(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.IronCuirassName,
                  ItemsLabeles.IronCuirassDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.IronCuirassBaseDurability,
                  GameBalance.IronCuirassBaseDamageBlock)
        { }
        private IronCuirass() { }
    }
}