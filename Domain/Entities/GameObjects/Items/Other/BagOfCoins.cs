using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class BagOfCoins : Item
    {
        public BagOfCoins(int id, int roomId)
            : base(id, ItemsLabeles.BagOfCoinsName,ItemsLabeles.BagOfCoinsDescription)
        {
            var (min, max) = GameBalance.CalculateSpread(GameBalance.BagOfCoinsBaseCost, roomId);
            Cost = Random.Shared.Next(min, max + 1);
        }
        private BagOfCoins() { }
    }
}