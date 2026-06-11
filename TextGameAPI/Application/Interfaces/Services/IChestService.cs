using Microsoft.AspNetCore.Mvc.Rendering;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;

namespace TextGame.Application.Interfaces.Services
{
    public interface IChestService
    {
        Chest GetChest(int chestId, IEnumerable<Item> items);
        bool OpenChest(Chest chest);
        void UnlockChest(Chest chest);
        IReadOnlyList<Item> SearchChest(Chest chest);
        void TakeItemFromChest(Chest chest, Item item);
        IReadOnlyList<Item> TakeAllItemsFromChest(Chest chest);
    }
}