using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Other
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