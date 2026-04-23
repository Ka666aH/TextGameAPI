using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class Map : Item
    {
        public Map(int id)
            : base(id, ItemsLabeles.MapName, ItemsLabeles.MapDescription)
        {
            Cost = GameBalance.MapBaseCost;
        }
        private Map() { }
    }
}