namespace TextGame.Domain.GameObjects.Items.Heal
{
    public class Bandage : Heal
    {
        public Bandage(int itemId, int roomId, bool fromShop)
            : base("ПОВЯЗКА", "Менее грязная тряпка, из тех, что здесь обычно встречаются.", itemId, roomId, fromShop, GameBalance.BandageBaseMaxHealthBoost, GameBalance.BandageBaseCurrentHealthBoost) { }
    }
}