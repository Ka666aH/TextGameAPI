using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class LeatherVest : Chestplate
    {
        public LeatherVest(int itemId, int roomId, bool fromShop)
            : base(ItemsLabeles.LeatherVestName,
                  ItemsLabeles.LeatherVestDescription,
                  itemId,
                  roomId,
                  fromShop,
                  GameBalance.LeatherVestBaseDurability,
                  GameBalance.LeatherVestBaseDamageBlock)
        { }
        private LeatherVest() { }
    }
}