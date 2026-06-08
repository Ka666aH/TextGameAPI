using TextGame.Application.DTO;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Orchestrators
{
    public interface IStateOrchestrator
    {
        Task<GameInfoDTO> GetGameInfoAsync(Guid sessionId, CancellationToken ct = default);

        Task<IEnumerable<Item>> GetInventoryAsync(Guid sessionId, CancellationToken ct = default);
        Task<int> GetCoinsAsync(Guid sessionId, CancellationToken ct = default);
        Task<int> GetKeysAsync(Guid sessionId, CancellationToken ct = default);
        Task<List<MapRoomDTO>> GetMapAsync(Guid sessionId, CancellationToken ct = default);
        Task UseInventoryItemAsync(int itemId, Guid sessionId, CancellationToken ct = default);

        Task<Item> GetInventoryItemAsync(int itemId, Guid sessionId, CancellationToken ct = default);

        Task<List<Equipment>> GetEquipmentAsync(Guid sessionId, CancellationToken ct = default);
        Task EquipInventoryItemAsync(int itemId, Guid sessionId, CancellationToken ct = default);
        Task UnequipWeaponAsync(Guid sessionId, CancellationToken ct = default);
        Task UnequipHelmAsync(Guid sessionId, CancellationToken ct = default);
        Task UnequipChestplateAsync(Guid sessionId, CancellationToken ct = default);
        Task SellInventoryItemAsync(int itemId, Guid sessionId, CancellationToken ct = default);


        Task<Room> GetCurrentRoomAsync(Guid sessionId, CancellationToken ct = default);
        Task<Room> GoNextRoomAsync(Guid sessionId, CancellationToken ct = default);
        Task<Room> GoToRoomAsync(int roomId, Guid sessionId, CancellationToken ct = default);
        Task<List<Item>> SearchAsync(Guid sessionId, CancellationToken ct = default);
        Task TakeItemAsync(int itemId, Guid sessionId, CancellationToken ct = default);
        Task TakeAllItemsAsync(Guid sessionId, CancellationToken ct = default);
        Task BuyItemAsync(int itemId, Guid sessionId, CancellationToken ct = default);

        Task<Enemy> GetEnemyAsync(Guid sessionId, CancellationToken ct = default);
        Task<BattleLog> DealDamageAsync(Guid sessionId, CancellationToken ct = default);
        Task<BattleLog> GetDamageAsync(Guid sessionId, CancellationToken ct = default);

        Task<BattleLog> HitChestAsync(int chestId, Guid sessionId, CancellationToken ct = default);
        Task<Chest> UnlockChestAsync(int chestId, Guid sessionId, CancellationToken ct = default);
        Task OpenChestAsync(int chestId, Guid sessionId, CancellationToken ct = default);
        Task<List<Item>> SearchChestAsync(int chestId, Guid sessionId, CancellationToken ct = default);
        Task TakeItemFromChestAsync(int chestId, int itemId, Guid sessionId, CancellationToken ct = default);
        Task TakeAllItemsFromChestAsync(int chestId, Guid sessionId, CancellationToken ct = default);
    }
}