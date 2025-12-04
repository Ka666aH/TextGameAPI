namespace TextGame
{
    public interface IGameSessionService
    {
        // Свойства для чтения состояния
        bool IsGameStarted { get; }
        bool IsInBattle { get; }
        Room CurrentRoom { get; }
        IReadOnlyList<Room> Rooms { get; }
        IReadOnlyList<Item> Inventory { get; }
        int Coins { get; }
        int Keys { get; }
        Weapon Weapon { get; }
        Helm? Helm { get; }
        Chestplate? Chestplate { get; }
        int MaxHealth { get; }
        int CurrentHealth { get; }
        Chest? CurrentMimicChest { get; }
        int RoomCounter { get; }
        int ItemCounter { get; }
        int EnemyCounter { get; }

        // Инкременты
        public int NextRoomNumber();
        public int NextItemId();
        public int NextEnemyId();
        //Equipment
        public void RemoveWeapon();
        public void EquipWeapon(Weapon weapon);
        public void RemoveChestplate();
        public void EquipChestplate(Chestplate chestplate);
        public void RemoveHelm();
        public void EquipHelm(Helm helm);

        //Health
        public void AddMaxHealth(int value);
        public void AddCurrentHealth(int value);

        public void RemoveCurrentMimicChest();
        public void SetCurrentMimicChest(Chest chest);

        public void StartGame();
        public void EndGame();
        public void StartBattle();
        public void EndBattle();

        public void AddCoins(int value);
        public void AddKeys(int value);
        public void AddItemToInventory(Item item);
        public void RemoveItemFromInventory(Item item);
        public void SetCurrentRoom(Room room);

        public void AddEnemyToCurrentRoom(Enemy enemy);
        public void RemoveEnemyFromCurrentRoom(Enemy enemy);
        public void AddItemToCurrentRoom(Item item);
        public void RemoveItemFromCurrentRoom(Item item);
    }
    public interface IMapGenerator
    {
        List<Room> Generate(IGameSessionService sessionService);
    }
    public interface IGetCurrentRoomRepository
    {
        Room GetCurrentRoom();
    }
    public interface ICheckItemService
    {
        void CheckItem(Item item);
    }
    public interface IChestRepository
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
    public interface IInventoryRepository
    {
        Item GetInventoryItem(int itemId);
        List<Equipment> EquipInventoryItem(int itemId);
        List<Equipment> GetEquipment();
        List<Equipment> UnequipWeapon();
        List<Equipment> UnequipHelm();
        List<Equipment> UnequipChestplate();
        void SellInventoryItem(int itemId);
        //List<Item> GetInventoryItems(List<int> itemIds);
    }
    public interface IGameInfoRepository
    {
        GameInfoDTO GetGameInfo();
    }
    public interface IGetRoomByIdRepository
    {
        Room GetRoomById(int roomId);
    }
    public interface IGetItemByIdRepository
    {
        Item GetItemById(int itemId, IEnumerable<Item> items);
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
        IEnumerable<Item> GetInventory();
        int GetCoins();
        int GetKeys();
        List<MapRoomDTO> GetMap();
        void UseInventoryItem(int itemId);

    }
    public interface IRoomControllerRepository :
        IGetCurrentRoomRepository, IChestRepository, IGetRoomByIdRepository, IGameInfoRepository, IGetEnemyByIdRepository, ICombatRepository
    {
        void GoNextRoom();
        IEnumerable<Item> Search();
        void TakeItem(int itemId);
        void TakeAllItems();
        void BuyItem(int itemId);
    }
}