using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Orchestrators;
using TextGame.Domain.DTO;
using TextGame.Domain.Entities.GameObjects.Enemies;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.RequireSession)]
    [Route("state")]
    public class StateController : ControllerBase
    {
        private readonly IStateOrchestrator _stateOrchestrator;

        public StateController(IStateOrchestrator stateOrchestrator)
        {
            _stateOrchestrator = stateOrchestrator;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInfoAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _stateOrchestrator.GetGameInfoAsync(sessionId, ct));
        }
        [HttpGet("map")]
        public async Task<IActionResult> GetMapAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _stateOrchestrator.GetMapAsync(sessionId, ct));
        }
        [HttpGet("coins")]
        public async Task<IActionResult> GetCoinsAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _stateOrchestrator.GetCoinsAsync(sessionId, ct));
        }
        [HttpGet("keys")]
        public async Task<IActionResult> GetKeysAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _stateOrchestrator.GetKeysAsync(sessionId, ct));
        }
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var items = await _stateOrchestrator.GetInventoryAsync(sessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpGet("inventory/{itemId}")]
        public async Task<IActionResult> GetInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var item = await _stateOrchestrator.GetInventoryItemAsync(itemId, sessionId, ct);
            return Ok(item.ToDTO());
        }
        [HttpPost("inventory/{itemId}/sell")]
        public async Task<IActionResult> SellInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _stateOrchestrator.SellInventoryItemAsync(itemId, sessionId, ct);
            return await GetInfoAsync(ct);
        }
        [HttpPost("inventory/{itemId}/use")]
        public async Task<IActionResult> UseInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _stateOrchestrator.UseInventoryItemAsync(itemId, sessionId, ct);
            return await GetInfoAsync(ct);
        }
        [HttpGet("equipment")]
        public async Task<IActionResult> GetEquipmentAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var equip = await _stateOrchestrator.GetEquipmentAsync(sessionId, ct);
            return Ok(equip.ToDTO());
        }
        [HttpPost("inventory/{itemId}/equip")]
        public async Task<IActionResult> EquipInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _stateOrchestrator.EquipInventoryItemAsync(itemId, sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/weapon/unequip")]
        public async Task<IActionResult> UnequipWeaponAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _stateOrchestrator.UnequipWeaponAsync(sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/helm/unequip")]
        public async Task<IActionResult> UnequipHelmAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _stateOrchestrator.UnequipHelmAsync(sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/chestplate/unequip")]
        public async Task<IActionResult> UnequipChestplateAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _stateOrchestrator.UnequipChestplateAsync(sessionId, ct);
            return await GetEquipmentAsync(ct);
        }

        [HttpPost("rooms/next")]
        public async Task<IActionResult> GoNextRoomAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var room = await _stateOrchestrator.GoNextRoomAsync(gameSessionId, ct);
            return Ok(room.ToDTO());
        }
        [HttpPost("rooms/{roomId}")]
        public async Task<IActionResult> GoRoomAsync(int roomId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var room = await _stateOrchestrator.GoToRoomAsync(roomId, gameSessionId, ct);
            return Ok(room.ToDTO());
        }
        [HttpGet("rooms/current")]
        public async Task<IActionResult> GetCurrentRoomAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var room = await _stateOrchestrator.GetCurrentRoomAsync(gameSessionId, ct);
            return Ok(room.ToDTO());
        }
        [HttpPost("rooms/current/items")]
        public async Task<IActionResult> SearchAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var items = await _stateOrchestrator.SearchAsync(gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpPost("rooms/current/items/{itemId}/take")]
        public async Task<IActionResult> TakeItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _stateOrchestrator.TakeItemAsync(itemId, gameSessionId, ct);
            var info = await _stateOrchestrator.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        [HttpPost("rooms/current/items/takeall")]
        public async Task<IActionResult> TakeAllItemsAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _stateOrchestrator.TakeAllItemsAsync(gameSessionId, ct);
            var info = await _stateOrchestrator.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        [HttpPost("rooms/current/items/{itemId}/buy")]
        public async Task<IActionResult> BuyItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _stateOrchestrator.BuyItemAsync(itemId, gameSessionId, ct);
            var info = await _stateOrchestrator.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        #region CHEST

        [HttpPost("rooms/current/items/{chestId}/chest/hit")]
        public async Task<IActionResult> HitChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            return Ok(await _stateOrchestrator.HitChestAsync(chestId, gameSessionId, ct));
        }
        [HttpPost("rooms/current/items/{chestId}/chest/open")]
        public async Task<IActionResult> OpenChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _stateOrchestrator.OpenChestAsync(chestId, gameSessionId, ct);
            var items = await _stateOrchestrator.SearchChestAsync(chestId, gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpPost("rooms/current/items/{chestId}/chest/unlock")]
        public async Task<IActionResult> UnlockChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var chest = await _stateOrchestrator.UnlockChestAsync(chestId, gameSessionId, ct);
            return Ok(chest.ToDTO());
        }
        [HttpPost("rooms/current/items/{chestId}/chest/items")]
        public async Task<IActionResult> SearchChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            var items = await _stateOrchestrator.SearchChestAsync(chestId, gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpPost("rooms/current/items/{chestId}/chest/items/{itemId}/take")]
        public async Task<IActionResult> TakeItemFromChestAsync(int chestId, int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _stateOrchestrator.TakeItemFromChestAsync(chestId, itemId, gameSessionId, ct);
            var info = await _stateOrchestrator.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        [HttpPost("rooms/current/items/{chestId}/chest/items/takeall")]
        public async Task<IActionResult> TakeAllItemsFromChestAsync(int chestId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            await _stateOrchestrator.TakeAllItemsFromChestAsync(chestId, gameSessionId, ct);
            var info = await _stateOrchestrator.GetGameInfoAsync(gameSessionId, ct);
            return Ok(info);
        }
        #endregion
        #region ENEMIES
        [HttpGet("rooms/current/enemy")]
        public async Task<IActionResult> GetEnemyAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            Enemy enemy = await _stateOrchestrator.GetEnemyAsync(gameSessionId, ct);
            return Ok(enemy.ToDTO());
        }
        [HttpPost("rooms/current/enemy/attack")]
        public async Task<IActionResult> AttackEnemyAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid gameSessionId);
            List<BattleLog> battleLogs = [
                await _stateOrchestrator.DealDamageAsync(gameSessionId, ct),
                await _stateOrchestrator.GetDamageAsync(gameSessionId, ct)];
            return Ok(battleLogs);
        }
        #endregion
    }
}