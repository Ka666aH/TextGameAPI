using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameText;

namespace TextGame.Domain.Entities.GameObjects.Items.Other
{
    public class BagOfCoins : Item
    {
        public BagOfCoins(int id, int roomId, Random? random = null)
            : base(id, ItemsLabels.BagOfCoinsName,ItemsLabels.BagOfCoinsDescription, roomId)
        {
            var rng = random ?? Random.Shared;
            var (min, max) = GameBalance.CalculateSpread(GameBalance.BagOfCoinsBaseCost, roomId);
            Cost = rng.Next(min, max + 1);
        }
        private BagOfCoins() { }
    }
}