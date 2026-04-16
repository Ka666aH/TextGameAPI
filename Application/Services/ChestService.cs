using TextGame.Application.Interfaces.Services;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameExceptions;
using TextGame.Domain.GameObjects.Items.Other;
using TextGame.Domain.GameText;

namespace TextGame.Application.Services
{
    public class ChestService : IChestService
    {
        private readonly IGetItemService _getItemByIdRepository;
        public ChestService(IGetItemService getItemByIdRepository)
        {
            _getItemByIdRepository = getItemByIdRepository;
        }
        public Chest GetChest(int chestId, IEnumerable<Item> items)
        {
            Item item = _getItemByIdRepository.GetItem(chestId, items);
            if (item is not Chest) throw new InvalidIdException(ExceptionLabels.NotChestCode, ExceptionLabels.NotChestText);
            return (Chest)item;
        }
        public bool OpenChest(Chest chest)
        {
            RequireUnlocked(chest);

            chest.Open();
            return chest.Mimic is null;
        }
        public void UnlockChest(Chest chest)
        {
            chest.Unlock();
        }
        public List<Item> SearchChest(Chest chest)
        {
            RequireUnlocked(chest);
            RequireOpened(chest);

            return chest.Search();
        }
        public void TakeItemFromChest(Chest chest, Item item)
        {
            RequireUnlocked(chest);
            RequireOpened(chest);

            chest.RemoveItem(item);
        }
        public List<Item> TakeAllItemsFromChest(Chest chest)
        {
            RequireUnlocked(chest);
            RequireOpened(chest);

            var items = chest.Items.ToList();
            chest.RemoveAllItems();
            return items;
        }
        private void RequireOpened(Chest chest)
        {
            if (chest.IsClosed) throw new ClosedException();
        }
        private void RequireUnlocked(Chest chest)
        {
            if (chest.IsLocked) throw new LockedException();
        }
    }
}