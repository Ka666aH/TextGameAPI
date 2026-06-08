using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.RequireSession)]
    [Route("rooms")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomControllerService _roomControllerService;

        public RoomController(IRoomControllerService roomControllerService)
        {
            _roomControllerService = roomControllerService;
        }
        [HttpPost("next")]
        public async Task<IActionResult> GoNextRoomAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var room = await _roomControllerService.GoNextRoomAsync(gameSessionId, ct);
            return Ok(room.ToDTO());
        }
        [HttpPost("{roomId}")]
        public async Task<IActionResult> GoRoomAsync(int roomId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var room = await _roomControllerService.GoToRoomAsync(roomId, gameSessionId, ct);
            return Ok(room.ToDTO());
        }
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentRoomAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var room = await _roomControllerService.GetCurrentRoomAsync(gameSessionId, ct);
            return Ok(room.ToDTO());
        }
        [HttpPost("current/items")]
        public async Task<IActionResult> SearchAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var items = await _roomControllerService.SearchAsync(gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpPost("current/items/{itemId}/take")]
        public async Task<IActionResult> TakeItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _roomControllerService.TakeItemAsync(itemId, gameSessionId, ct);
            var info = await _roomControllerService.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        [HttpPost("current/items/takeall")]
        public async Task<IActionResult> TakeAllItemsAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _roomControllerService.TakeAllItemsAsync(gameSessionId, ct);
            var info = await _roomControllerService.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        [HttpPost("current/items/{itemId}/buy")]
        public async Task<IActionResult> BuyItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _roomControllerService.BuyItemAsync(itemId, gameSessionId, ct);
            var info = await _roomControllerService.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        #region CHEST

        [HttpPost("current/items/{chestId}/chest/hit")]
        public async Task<IActionResult> HitChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            return Ok(await _roomControllerService.HitChestAsync(chestId, gameSessionId, ct));
        }
        [HttpPost("current/items/{chestId}/chest/open")]
        public async Task<IActionResult> OpenChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _roomControllerService.OpenChestAsync(chestId, gameSessionId, ct);
            var items = await _roomControllerService.SearchChestAsync(chestId, gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpPost("current/items/{chestId}/chest/unlock")]
        public async Task<IActionResult> UnlockChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var chest = await _roomControllerService.UnlockChestAsync(chestId, gameSessionId, ct);
            return Ok(chest.ToDTO());
        }
        [HttpPost("current/items/{chestId}/chest/items")]
        public async Task<IActionResult> SearchChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var items = await _roomControllerService.SearchChestAsync(chestId, gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpPost("current/items/{chestId}/chest/items/{itemId}/take")]
        public async Task<IActionResult> TakeItemFromChestAsync(int chestId, int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _roomControllerService.TakeItemFromChestAsync(chestId, itemId, gameSessionId, ct);
            var info = await _roomControllerService.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        [HttpPost("current/items/{chestId}/chest/items/takeall")]
        public async Task<IActionResult> TakeAllItemsFromChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _roomControllerService.TakeAllItemsFromChestAsync(chestId, gameSessionId, ct);
            var info = await _roomControllerService.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        #endregion
        #region ENEMIES
        [HttpGet("current/enemy")]
        public async Task<IActionResult> GetEnemyAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            Enemy enemy = await _roomControllerService.GetEnemyAsync(gameSessionId, ct);
            return Ok(enemy.ToDTO());
        }
        [HttpPost("current/enemy/attack")]
        public async Task<IActionResult> AttackEnemyAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            List<BattleLog> battleLogs = [
                await _roomControllerService.DealDamageAsync(gameSessionId, ct),
                await _roomControllerService.GetDamageAsync(gameSessionId, ct)];
            return Ok(battleLogs);
        }
        #endregion
    }
}