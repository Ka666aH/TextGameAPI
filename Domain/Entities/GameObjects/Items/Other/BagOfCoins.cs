using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class BagOfCoins : Item
    {
        public BagOfCoins(int itemId, int roomId)
            : base(ItemsLabeles.BagOfCoinsName,ItemsLabeles.BagOfCoinsDescription, itemId)
        {
            var (min, max) = GameBalance.CalculateSpread(GameBalance.BagOfCoinsBaseCost, roomId);
            Cost = Random.Shared.Next(min, max + 1);
        }
    }
}