using TextGame.Application.Interfaces.Factories;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Enemies;

namespace TextGame.Domain.GameObjects.Items
{
    public class Chest : Item
    {
        public bool IsLocked { get; set; }
        public bool IsClosed { get; set; } = true;
        public Mimic? Mimic { get; private set; }
        public readonly List<Item> _items = new();
        public IReadOnlyList<Item> Items => _items.AsReadOnly();

        public Chest(IItemFactory itemFactory, IEnemyFactory enemyFactory, IGameSessionService sessionService)
            : base("СУНДУК", "Хранит предметы. Может оказаться мимиком.", sessionService.NextItemId(), false)
        {
            Cost = null;
            IsLocked = Random.Shared.Next(GameBalance.ChestDivider) < GameBalance.LockedProbabilityDenominator;
            Mimic = Random.Shared.Next(GameBalance.ChestDivider) < GameBalance.MimicProbabilityDenominator ? enemyFactory.CreateMimic(sessionService) : null;
            var itemsInChest = Random.Shared.Next(GameBalance.MinChestItemsAmount, GameBalance.MaxChestItemsAmount + 1);
            for (int i = 0; i < itemsInChest; i++)
            {
                Item? item = itemFactory.CreateChestItem(sessionService);
                if (item != null) AddItem(item);
            }
        }
        public void AddItem(Item item) => _items.Add(item);
        public void RemoveItem(Item item)
        {
            if (_items.Contains(item)) _items.Remove(item);
            else throw new NullItemIdException();
        }
        public void Open() => IsClosed = false;
        public void Unlock() => IsLocked = false;
        public IEnumerable<Item> Search() => Items;
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