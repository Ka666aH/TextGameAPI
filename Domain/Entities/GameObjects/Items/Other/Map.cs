using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class Map : Item
    {
        public Map(int itemId)
            : base(ItemsLabeles.MapName, ItemsLabeles.MapDescription, itemId)
        {
            Cost = GameBalance.MapBaseCost;
        }
    }
}