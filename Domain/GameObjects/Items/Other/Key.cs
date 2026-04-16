using TextGame.Domain.GameText;

namespace TextGame.Domain.GameObjects.Items.Other
{
    public class Key : Item
    {
        public Key(int itemId, int roomId)
            : base(ItemsLabeles.KeyName, ItemsLabeles.KeyDescription, itemId)
        {
            Cost = (int)(GameBalance.KeyBaseCost * GameBalance.CalculateGain(roomId));
        }
    }
}