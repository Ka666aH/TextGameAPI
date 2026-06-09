using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class LeatherVest : Chestplate
    {
        public LeatherVest(int id, int roomId, bool fromShop)
            : base(id,
                  ItemsLabels.LeatherVestName,
                  ItemsLabels.LeatherVestDescription,
                  roomId,
                  fromShop,
                  GameBalance.LeatherVestBaseDurability,
                  GameBalance.LeatherVestBaseDamageBlock)
        { }
        private LeatherVest() { }
    }
}