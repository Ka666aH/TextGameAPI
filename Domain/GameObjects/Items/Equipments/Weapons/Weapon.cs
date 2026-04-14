namespace TextGame.Domain.GameObjects.Items.Equipments.Weapons
{
    public abstract class Weapon : Equipment
    {
        public int Damage { get; protected set; }

        public Weapon(string name, string description, int? id, int? durability, int damage, int roomId, bool fromShop)
            : base(name, description, id, durability, roomId, fromShop)
        {
            Damage = damage;
        }
        public abstract WeaponAttackResult Attack(int roomId);
    }
}
