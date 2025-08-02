using static TextGame.GameRepository;

namespace TextGame
{
    public interface IGameRepository
    {

        void Start();
        void GoNextRoom();
        CurrentRoomDTO ShowCurrentRoom();
        List<Item> Search();
        void TakeItem(int id);
        void TakeAllItems();
        //bool IsChestExist(int id);
        bool CheckChest(int id);
        bool UnlockChest(int id);
        List<Item> OpenChest(int id);
        void TakeItemFromChest(int chestId, int itemId);
        void TakeAllItemsFromChest(int chestId);
        List<Item> ShowInventory();
        int ShowCoins();
    }
}
