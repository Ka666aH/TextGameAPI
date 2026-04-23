using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.GameExceptions;

namespace TextGame.Domain.Entities.GameObjects.Rooms
{
    public abstract class Room : GameObject
    {
        private readonly List<Item> _items = [];
        public IReadOnlyList<Item> Items => _items;
        private readonly List<Enemy> _enemies = [];
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public Enemy? Enemy => _enemies.FirstOrDefault();
        public bool IsDiscovered { get; protected set; } = false;
        public bool IsSearched { get; protected set; } = false;

        public Room(int id, string name, string description) : base(id, name, description) { }

        public void AddItem(Item item) => _items.Add(item);
        public void RemoveItem(Item item)
        {
            if (_items.Contains(item)) _items.Remove(item);
            else throw new NullItemIdException();
        }
        public void AddEnemy(Enemy enemy) => _enemies.Add(enemy);
        public void RemoveEnemy(Enemy enemy)
        {
            if (!_enemies.Remove(enemy)) throw new NullEnemyIdException();
        }
        public List<Item> Search()
        {
            IsSearched = true;
            return _items;
        }
        public void Discover() => IsDiscovered = true;
        protected Room() { }
    }
}