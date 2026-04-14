namespace TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates
{
    public abstract class Chestplate : Armor
    {
        public Chestplate(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damageBlock)
            : base(name, description, itemId, roomId, fromShop, durability, damageBlock) { }
    }
}