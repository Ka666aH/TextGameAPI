namespace TextGame.Domain.GameObjects.Items.Other
{
    public class Map : Item
    {
        public Map(int itemId)
            : base("КАРТА", "Содержит знания о строении подземелья.", itemId)
        {
            Cost = GameBalance.MapBaseCost;
        }
    }
}