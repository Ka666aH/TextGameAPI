using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms
{
    public abstract class Helm : Armor
    {
        public Helm(int id, string name, string description, int roomId, bool fromShop, int durability, int damageBlock)
            : base(id, name, description, roomId, fromShop, durability, damageBlock) { }
        protected Helm() { }
    }
}