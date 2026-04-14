namespace TextGame.Domain.GameObjects.Items
{
    public class Key : Item
    {
        public Key(int itemId, int roomId)
            : base("КЛЮЧ", "Непрочный продолговатый кусок металла. Что-то открывает.", itemId, true)
        {
            Cost = (int)(GameBalance.KeyBaseCost * GameBalance.ApplyGain(roomId));
        }
    }
}