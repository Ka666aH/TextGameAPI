using TextGame.Domain.GameObjects.Items;
using TextGame.Presentation.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IChestService
    {
        ChestStateDTO ReturnChestDTO(Chest chest);
        ChestStateDTO ReturnChestDTO(int chestId);
        BattleLog HitChest(int chestId);
        void OpenChest(int chestId);
        void UnlockChest(int chestId);
        IEnumerable<Item> SearchChest(int chestId);
        void TakeItemFromChest(int chestId, int itemId);
        void TakeAllItemsFromChest(int chestId);
    }
}