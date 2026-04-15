using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Domain.GameObjects.Items.Other
{
    public class Chest : Item
    {
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; } = true;
        public Mimic? Mimic { get; private set; }
        public readonly List<Item> _items = new();
        public IReadOnlyList<Item> Items => _items.AsReadOnly();

        public Chest(int itemId, List<Item> items, Mimic? mimic = null)
            : base("СУНДУК", "Хранит предметы. Может оказаться мимиком.", itemId, false)
        {
            Cost = null;
            IsLocked = Random.Shared.Next(GameBalance.ChestDivider) < GameBalance.LockedProbabilityDenominator;
            Mimic = mimic;
            _items = items;
        }
        //public void AddItem(Item item) => _items.Add(item);
        public void RemoveItem(Item item)
        {
            if (_items.Contains(item)) _items.Remove(item);
            else throw new NullItemIdException();
        }
        public void RemoveAllItems()
        {
            _items.Clear();
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public List<Item> Search() => Items.ToList();
        public void KillMimic()
        {
            Mimic = null;
            Name = "МЁРТВЫЙ МИМИК";
            Description = "Мёртвый сундук с руками и зубами. Интересно, что у него внутри.";
            IsLocked = false;
            IsClosed = false;
        }
    }
}