namespace TextGame
{
    public interface IGetCurrentRoomRepository
    {
        Room GetCurrentRoom();
    }
    public interface IChestRepository
    {
        ChestDTO ReturnChestDTO(Chest chest);
        ChestDTO ReturnChestDTO(int chestId);
        BattleLog HitChest(int chestId);
        void OpenChest(int chestId);
        void UnlockChest(int chestId);
        List<Item> SearchChest(int chestId);
        void TakeItemFromChest(int chestId, int itemId);
        void TakeAllItemsFromChest(int chestId);
    }
    public interface IInventoryRepository
    {
        Item GetInventoryItem(int itemId);
        List<Equipment> EquipInventoryItem(int itemId);
        List<Equipment> GetEquipment();
        List<Equipment> UnequipWeapon();
        List<Equipment> UnequipHelm();
        List<Equipment> UnequipChestplate();
        //List<Item> GetInventoryItems(List<int> itemIds);
    }
    public interface IGameInfoRepository
    {
        GameInfoDTO GetGameInfo();
    }
    public interface IGameOverInfoRepository
    {
        GameOvernInfoDTO GetGameOverInfo();
    }
    public interface IGetRoomByIdRepository
    {
        Room GetRoomById(int roomId);
    }
    public interface IGetItemByIdRepository
    {
        Item GetItemById(int itemId, List<Item> items);
    }
    public interface IGetEnemyByIdRepository
    {
        Enemy GetEnemyById();
        //List<Enemy> GetEnemies();
    }
    public interface ICombatRepository
    {
        BattleLog DealDamage();
        BattleLog GetDamage();
    }
    public interface IGameControllerRepository : IGetCurrentRoomRepository, IInventoryRepository, IGameInfoRepository
    {
        void Start();
        List<Item> GetInventory();
        int GetCoins();
        int GetKeys();
        List<MapRoomDTO> GetMap();

    }
    public interface IRoomControllerRepository :
        IGetCurrentRoomRepository, IChestRepository, IGetRoomByIdRepository, IGameInfoRepository, IGetEnemyByIdRepository, ICombatRepository
    {
        void GoNextRoom();
        List<Item> Search();
        void TakeItem(int itemId);
        void TakeAllItems();
    }
}