using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments;
using TextGame.Domain.Entities.GameObjects.Rooms;
using TextGame.Presentation.DTO;

namespace TextGame.Application.Interfaces.Services
{
    public interface IGameControllerService
    {
        //void Start();
        //Task EnsureGameSessionLoadedAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<Room> GetCurrentRoomAsync(Guid gameSessionId, CancellationToken ct = default);

        Task<IEnumerable<Item>> GetInventoryAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<int> GetCoinsAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<int> GetKeysAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<List<MapRoomDTO>> GetMapAsync(Guid gameSessionId, CancellationToken ct = default);
        Task UseInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default);

        Task<Item> GetInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default);

        Task<List<Equipment>> GetEquipmentAsync(Guid gameSessionId, CancellationToken ct = default);
        Task EquipInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default);
        Task UnequipWeaponAsync(Guid gameSessionId, CancellationToken ct = default);
        Task UnequipHelmAsync(Guid gameSessionId, CancellationToken ct = default);
        Task UnequipChestplateAsync(Guid gameSessionId, CancellationToken ct = default);
        Task SellInventoryItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default);

        Task<GameInfoDTO> GetGameInfoAsync(Guid gameSessionId, CancellationToken ct = default);
    }
}