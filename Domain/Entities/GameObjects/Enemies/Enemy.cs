using TextGame.Domain.Entities.GameObjects;

namespace TextGame.Domain.Entities.GameObjects.Enemies
{
    public abstract class Enemy : GameObject
    {
        public int Id { get; protected set; }
        public int Health { get; protected set; } = 0;
        public int Damage { get; protected set; } = 0;
        public int DamageBlock { get; protected set; } = 0;

        private readonly int _roomId;

        public Enemy(string name, string description, int roomId, int id, int health, int damage, int damageBlock)
            : base(name, description)
        {
            Id = id;
            _roomId = roomId;
            Initialize(health, damage, damageBlock);
        }
        public virtual void Initialize(int health, int damage, int damageBlock)
        {
            var (minHealth, maxHealth) = GameBalance.CalculateSpread(health, _roomId);
            Health = Random.Shared.Next(minHealth, maxHealth + 1);

            var (minDamage, maxDamage) = GameBalance.CalculateSpread(damage, _roomId);
            Damage = Random.Shared.Next(minDamage, maxDamage + 1);

            var (minDamageBlock, maxDamageBlock) = GameBalance.CalculateSpread(damageBlock, _roomId);
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