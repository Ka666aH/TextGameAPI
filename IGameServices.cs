namespace TextGame
{
    public interface IGetCurrentRoomRepository
    {
        Room GetCurrentRoom();
    }
    public interface IChestRepository
    {
        ChestDTO ReturnChestDTO(Chest chest);
        ChestDTO ReturnChestDTO(int roomId, int chestId);
        void OpenChest(int roomId, int chestId);
        void UnlockChest(int roomId, int chestId);
        List<Item> SearchChest(int roomId, int chestId);
        void TakeItemFromChest(int roomId, int chestId, int itemId);
        void TakeAllItemsFromChest(int roomId, int chestId);
    }
    public interface IInventoryRepository
    {
        Item GetInventoryItem(int itemId);
        EquipmentListDTO EquipInventoryItem(int itemId);
        EquipmentListDTO GetEquipment();
        EquipmentListDTO UnequipWeapon();
        EquipmentListDTO UnequipHelm();
        EquipmentListDTO UnequipChestplate();
        //List<Item> GetInventoryItems(List<int> itemIds);
    }
    public interface IGameStatsRepository
    {
        GameStatsDTO GetGameStats();
    }
    public interface IGameOverStatsRepository
    {
        GameOverStatsDTO GetGameOverStats();
    }
    public interface IGetRoomByIdRepository
    {
        Room GetRoomById(int roomId);
    }
    public interface IGetItemByIdRepository
    {
        Item GetItemById(int itemId, List<Item> items);
    }
    public interface IGameControllerRepository : IGetCurrentRoomRepository, IInventoryRepository
    {
        void Start();
        List<Item> GetInventory();
        int GetCoins();
        int GetKeys();
        List<MapRoomDTO> GetMap();

    }
    public interface IRoomControllerRepository : IGetCurrentRoomRepository, IChestRepository, /*IInventoryRepository,*/ IGetRoomByIdRepository,IGameStatsRepository
    {
        void GoNextRoom();
        List<Item> Search(int roomId);
        void TakeItem(int roomId, int itemId);
        void TakeAllItems(int roomId);
    }
}