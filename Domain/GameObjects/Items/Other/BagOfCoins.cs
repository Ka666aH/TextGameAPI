namespace TextGame.Domain.GameObjects.Items.Other
{
    public class BagOfCoins : Item
    {
        public BagOfCoins(int itemId, int roomId)
            : base("МЕШОЧЕК С МОНЕТАМИ", "Потрёпанный кусок ткани с разными монетами внутри.", itemId)
        {
            var (min, max) = GameBalance.CalculateSpread(GameBalance.BagOfCoinsBaseCost, roomId);
            Cost = Random.Shared.Next(min, max + 1);
        }
    }
}