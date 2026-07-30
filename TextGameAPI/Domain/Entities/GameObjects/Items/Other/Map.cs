using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class Map : Item
    {
        public Map(int id, int roomId)
            : base(id, ItemsLabels.MapName, ItemsLabels.MapDescription, roomId)
        {
            Cost = GameBalance.MapBaseCost;
        }
        private Map() { }
    }
}