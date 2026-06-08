using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.RequireSession)]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly IGameControllerService _gameControllerService;
        public GameController(IGameControllerService gameControllerRepository)
        {
            _gameControllerService = gameControllerRepository;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInfoAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _gameControllerService.GetGameInfoAsync(sessionId, ct));
        }
        [HttpGet("map")]
        public async Task<IActionResult> GetMapAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _gameControllerService.GetMapAsync(sessionId, ct));
        }
        [HttpGet("coins")]
        public async Task<IActionResult> GetCoinsAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _gameControllerService.GetCoinsAsync(sessionId, ct));
        }
        [HttpGet("keys")]
        public async Task<IActionResult> GetKeysAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            return Ok(await _gameControllerService.GetKeysAsync(sessionId, ct));
        }
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var items = await _gameControllerService.GetInventoryAsync(sessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpGet("inventory/{itemId}")]
        public async Task<IActionResult> GetInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var item = await _gameControllerService.GetInventoryItemAsync(itemId, sessionId, ct);
            return Ok(item.ToDTO());
        }
        [HttpPost("inventory/{itemId}/sell")]
        public async Task<IActionResult> SellInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _gameControllerService.SellInventoryItemAsync(itemId, sessionId, ct);
            return await GetInfoAsync(ct);
        }
        [HttpPost("inventory/{itemId}/use")]
        public async Task<IActionResult> UseInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _gameControllerService.UseInventoryItemAsync(itemId, sessionId, ct);
            return await GetInfoAsync(ct);
        }
        [HttpGet("equipment")]
        public async Task<IActionResult> GetEquipmentAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            var equip = await _gameControllerService.GetEquipmentAsync(sessionId, ct);
            return Ok(equip.ToDTO());
        }
        [HttpPost("inventory/{itemId}/equip")]
        public async Task<IActionResult> EquipInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _gameControllerService.EquipInventoryItemAsync(itemId, sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/weapon/unequip")]
        public async Task<IActionResult> UnequipWeaponAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _gameControllerService.UnequipWeaponAsync(sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/helm/unequip")]
        public async Task<IActionResult> UnequipHelmAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _gameControllerService.UnequipHelmAsync(sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/chestplate/unequip")]
        public async Task<IActionResult> UnequipChestplateAsync(CancellationToken ct)
        {
            User.TryGetSessionId(out Guid sessionId);
            await _gameControllerService.UnequipChestplateAsync(sessionId, ct);
            return await GetEquipmentAsync(ct);
        }
    }
}