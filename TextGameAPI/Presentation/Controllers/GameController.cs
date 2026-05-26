using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Helpers;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.Options;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.RequireGameSession)]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly IGameControllerService _gameControllerService;
        public GameController(IGameControllerService gameControllerRepository)
        {
            _gameControllerService = gameControllerRepository;
        }

        //[HttpPost("start")]
        //public IActionResult Start()
        //{
        //    //_gameControllerService.Start();
        //    var room = _gameControllerService.GetCurrentRoom();
        //    return Ok(room.ToDTO());
        //}
        [HttpGet("info")]
        public async Task<IActionResult> GetInfoAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            return Ok(await _gameControllerService.GetGameInfoAsync(gameSessionId, ct));
        }
        [HttpGet("map")]
        public async Task<IActionResult> GetMapAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            return Ok(await _gameControllerService.GetMapAsync(gameSessionId, ct));
        }
        [HttpGet("coins")]
        public async Task<IActionResult> GetCoinsAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            return Ok(await _gameControllerService.GetCoinsAsync(gameSessionId, ct));
        }
        [HttpGet("keys")]
        public async Task<IActionResult> GetKeysAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            return Ok(await _gameControllerService.GetKeysAsync(gameSessionId, ct));
        }
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            var items = await _gameControllerService.GetInventoryAsync(gameSessionId, ct);
            return Ok(items.ToDTO());
        }
        [HttpGet("inventory/{itemId}")]
        public async Task<IActionResult> GetInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            var item = await _gameControllerService.GetInventoryItemAsync(itemId, gameSessionId, ct);
            return Ok(item.ToDTO());
        }
        [HttpPost("inventory/{itemId}/sell")]
        public async Task<IActionResult> SellInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _gameControllerService.SellInventoryItemAsync(itemId, gameSessionId, ct);
            return await GetInfoAsync(ct);
        }
        [HttpPost("inventory/{itemId}/use")]
        public async Task<IActionResult> UseInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _gameControllerService.UseInventoryItemAsync(itemId, gameSessionId, ct);
            return await GetInfoAsync(ct);
        }
        [HttpGet("equipment")]
        public async Task<IActionResult> GetEquipmentAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            var equip = await _gameControllerService.GetEquipmentAsync(gameSessionId, ct);
            return Ok(equip.ToDTO());
        }
        [HttpPost("inventory/{itemId}/equip")]
        public async Task<IActionResult> EquipInventoryItemAsync(int itemId, CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _gameControllerService.EquipInventoryItemAsync(itemId, gameSessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/weapon/unequip")]
        public async Task<IActionResult> UnequipWeaponAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _gameControllerService.UnequipWeaponAsync(gameSessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/helm/unequip")]
        public async Task<IActionResult> UnequipHelmAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _gameControllerService.UnequipHelmAsync(gameSessionId, ct);
            return await GetEquipmentAsync(ct);
        }
        [HttpPost("equipment/chestplate/unequip")]
        public async Task<IActionResult> UnequipChestplateAsync(CancellationToken ct)
        {
            User.TryGetGameSessionId(out Guid gameSessionId);
            await _gameControllerService.UnequipChestplateAsync(gameSessionId, ct);
            return await GetEquipmentAsync(ct);
        }
    }
}