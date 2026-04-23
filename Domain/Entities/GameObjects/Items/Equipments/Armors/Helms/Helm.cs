using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public abstract class Helm : Armor
    {
        public Helm(string name, string description, int itemId, int roomId, bool fromShop, int durability, int damageBlock)
            : base(name, description, itemId, roomId, fromShop, durability, damageBlock) { }
        protected Helm() { }
    }
}