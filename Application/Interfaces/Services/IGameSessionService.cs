using TextGame.Domain.GameObjects.Enemies;
using TextGame.Domain.GameObjects.Items;
using TextGame.Domain.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Services
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

        public List<Item> SearchCurrentRoom();
    }
}