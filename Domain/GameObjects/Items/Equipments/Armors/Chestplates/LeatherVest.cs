namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates
{
    public class LeatherVest : Chestplate
    {
        public LeatherVest(int itemId, int roomId, bool fromShop)
            : base("КОЖАННАЯ КУРТКА", "Лёгкая куртка из плотной кожи.", itemId, roomId, fromShop, GameBalance.LeatherVestBaseDurability, GameBalance.LeatherVestBaseDamageBlock) { }

    }
}