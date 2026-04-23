using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class IronCuirass : Chestplate
    {
        public IronCuirass(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabeles.IronCuirassName,
                  ItemsLabeles.IronCuirassDescription,
                  roomId,
                  fromShop,
                  GameBalance.IronCuirassBaseDurability,
                  GameBalance.IronCuirassBaseDamageBlock)
        { }
        private IronCuirass() { }
    }
}