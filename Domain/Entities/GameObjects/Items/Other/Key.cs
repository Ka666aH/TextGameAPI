using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class Key : Item
    {
        public Key(int itemId, int roomId)
            : base(ItemsLabeles.KeyName, ItemsLabeles.KeyDescription, itemId)
        {
            Cost = (int)(GameBalance.KeyBaseCost * GameBalance.CalculateGain(roomId));
        }
        private Key() { }
    }
}