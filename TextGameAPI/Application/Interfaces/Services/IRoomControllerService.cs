using TextGame.Application.DTO;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Other;
using TextGame.Domain.Entities.GameObjects.Rooms;

namespace TextGame.Application.Interfaces.Services
{
    public interface IRoomControllerService
    {
        Task<Room> GetCurrentRoomAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<Room> GoNextRoomAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<Room> GoToRoomAsync(int roomId, Guid gameSessionId, CancellationToken ct = default);
        Task<List<Item>> SearchAsync(Guid gameSessionId, CancellationToken ct = default);
        Task TakeItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default);
        Task TakeAllItemsAsync(Guid gameSessionId, CancellationToken ct = default);
        Task BuyItemAsync(int itemId, Guid gameSessionId, CancellationToken ct = default);

        Task<Enemy> GetEnemyAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<BattleLog> DealDamageAsync(Guid gameSessionId, CancellationToken ct = default);
        Task<BattleLog> GetDamageAsync(Guid gameSessionId, CancellationToken ct = default);

        Task<GameInfoDTO> GetGameInfoAsync(Guid gameSessionId, CancellationToken ct = default);

        Task<BattleLog> HitChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default);
        Task<Chest> UnlockChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default);
        Task OpenChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default);
        Task<List<Item>> SearchChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default);
        Task TakeItemFromChestAsync(int chestId, int itemId, Guid gameSessionId, CancellationToken ct = default);
        Task TakeAllItemsFromChestAsync(int chestId, Guid gameSessionId, CancellationToken ct = default);
    }
}