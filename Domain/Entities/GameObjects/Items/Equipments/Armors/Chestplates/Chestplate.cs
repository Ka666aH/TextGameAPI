using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates
{
    public abstract class Chestplate : Armor
    {
        public Chestplate(int id, string name, string description, int roomId, bool fromShop, int durability, int damageBlock)
            : base(id, name, description, roomId, fromShop, durability, damageBlock) { }
        protected Chestplate() { }
    }
}