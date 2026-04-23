using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public abstract class Enemy : GameObject
    {
        public int RoomId { get; protected set; }
        public Room? Room { get; protected set; }
        public int Health { get; protected set; } = 0;
        public int Damage { get; protected set; } = 0;
        public int DamageBlock { get; protected set; } = 0;


        public Enemy(int id, string name, string description, int roomId, int health, int damage, int damageBlock)
            : base(id, name, description)
        {
            RoomId = roomId;
            Initialize(health, damage, damageBlock);
        }
        public virtual void Initialize(int health, int damage, int damageBlock)
        {
            var (minHealth, maxHealth) = GameBalance.CalculateSpread(health, RoomId);
            Health = Random.Shared.Next(minHealth, maxHealth + 1);

            var (minDamage, maxDamage) = GameBalance.CalculateSpread(damage, RoomId);
            Damage = Random.Shared.Next(minDamage, maxDamage + 1);

            var (minDamageBlock, maxDamageBlock) = GameBalance.CalculateSpread(damageBlock, RoomId);
            DamageBlock = Random.Shared.Next(minDamageBlock, maxDamageBlock + 1);
        }
        public virtual int Attack() => Damage;
        public virtual int GetDamage(int damage)
        {
            if (damage > DamageBlock) Health -= damage - DamageBlock;
            return Health;
        }
        protected Enemy() { }
    }
}