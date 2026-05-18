using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class Map : Item
    {
        public Map(int id, int roomId)
            : base(id, ItemsLabeles.MapName, ItemsLabeles.MapDescription, roomId)
        {
            Cost = GameBalance.MapBaseCost;
        }
        private Map() { }
    }
}