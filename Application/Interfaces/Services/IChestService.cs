using Microsoft.AspNetCore.Mvc.Rendering;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Other;

namespace TextGame.Application.Interfaces.Services
{
    public interface IChestService
    {
        Chest GetChest(int chestId, IEnumerable<Item> items);
        bool OpenChest(Chest chest);
        void UnlockChest(Chest chest);
        List<Item> SearchChest(Chest chest);
        void TakeItemFromChest(Chest chest, Item item);
        List<Item> TakeAllItemsFromChest(Chest chest);
    }
}