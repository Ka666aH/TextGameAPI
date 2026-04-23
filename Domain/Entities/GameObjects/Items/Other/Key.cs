using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class Key : Item
    {
        public Key(int id, int roomId)
            : base(id, ItemsLabeles.KeyName, ItemsLabeles.KeyDescription, roomId)
        {
            Cost = (int)(GameBalance.KeyBaseCost * GameBalance.CalculateGain(roomId));
        }
        private Key() { }
    }
}