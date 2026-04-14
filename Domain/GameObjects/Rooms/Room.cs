using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;

namespace TextGame.Domain.GameObjects.Rooms
{
    public abstract class Room : GameObject
    {
        public int Number { get; init; }
        private readonly List<Item> _items = new();
        public IReadOnlyList<Item> Items => _items;
        private readonly List<Enemy> _enemies = new();
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public bool IsDiscovered { get; set; } = false;
        public bool IsSearched { get; set; } = false;

        public Room(string name, string description, int number)
        {
            Name = name;
            Description = description;
            Number = number;
        }
        public void AddItem(Item item) => _items.Add(item);
        public void RemoveItem(Item item)
        {
            if (_items.Contains(item)) _items.Remove(item);
            else throw new NullItemIdException();
        }
        public void AddEnemy(Enemy enemy) => _enemies.Add(enemy);
        public void RemoveEnemy(Enemy enemy)
        {

            if (_enemies.Contains(enemy)) _enemies.Remove(enemy);
            else throw new NullEnemyIdException();
        }
        public List<Item> Search()
        {
            IsSearched = true;
            return _items;
        }

    }
}
