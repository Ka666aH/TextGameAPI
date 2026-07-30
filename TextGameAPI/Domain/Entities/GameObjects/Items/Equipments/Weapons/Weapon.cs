using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;

namespace TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons
{
    public abstract class Weapon : Equipment
    {
        public int Damage { get; protected set; }

        public Weapon(int id, string name, string description, int? durability, int damage, int roomId, bool fromShop)
            : base(id, name, description, durability, roomId, fromShop)
        {
            Damage = damage;
        }
        public abstract WeaponAttackResult Attack(int roomId);
        protected Weapon() { }
    }
}
