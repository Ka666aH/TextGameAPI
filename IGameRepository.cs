using static TextGame.GameRepository;

namespace TextGame
{
    public interface IGameRepository
    {

        void Start();
        void GoNextRoom();
        CurrentRoomDTO ShowCurrentRoom();
        List<Item> Search(int roomId);
        void TakeItem(int roomId, int itemId);
        void TakeAllItems(int roomId);
        //bool IsChestExist(int id);
        //bool CheckChest(int id);
        ChestDTO ReturnChestDTO(Chest chest);
        ChestDTO ReturnChestDTO(int roomId, int chestId);
        void OpenChest(int roomId, int chestId);
        void UnlockChest(int roomId, int chestId);
        List<Item> SearchChest(int roomId, int chestId);
        void TakeItemFromChest(int roomId, int chestId, int itemId);
        void TakeAllItemsFromChest(int roomId, int chestId);
        List<Item> ShowInventory();
        Item ShowInventoryItem(int itemId);
        List<Item> ShowInventoryItems(List<int> itemIds);
        int ShowCoins();
        GameOverStatsDTO ShowGameOverStats();
    }
}
