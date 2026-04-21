using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Mappers;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly IGameControllerService _gameControllerService;

        public GameController(IGameControllerService gameControllerRepository)
        {
            _gameControllerService = gameControllerRepository;
        }

        [HttpPost("start")]
        public IActionResult Start()
        {
            _gameControllerService.Start();
            var room = _gameControllerService.GetCurrentRoom();
            return Ok(GameObjectMapper.ToDTO(room));
        }
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(_gameControllerService.GetGameInfo());
        }
        [HttpGet("map")]
        public IActionResult GetMap()
        {
            return Ok(_gameControllerService.GetMap());
        }
        [HttpGet("coins")]
        public IActionResult GetCoins()
        {
            return Ok(_gameControllerService.GetCoins());
        }
        [HttpGet("keys")]
        public IActionResult GetKeys()
        {
            return Ok(_gameControllerService.GetKeys());
        }
        [HttpGet("inventory")]
        public IActionResult GetInventory()
        {
            return Ok(_gameControllerService.GetInventory());
        }
        [HttpGet("inventory/{itemId}")]
        public IActionResult GetInventoryItem(int itemId)
        {
            return Ok(_gameControllerService.GetInventoryItem(itemId));
        }
        [HttpPost("inventory/{itemId}/sell")]
        public IActionResult SellInventoryItem(int itemId)
        {
            _gameControllerService.SellInventoryItem(itemId);
            return GetInfo();
        }
        [HttpPost("inventory/{itemId}/use")]
        public IActionResult UseInventoryItem(int itemId)
        {
            _gameControllerService.UseInventoryItem(itemId);
            return GetInfo();
        }
        [HttpGet("equipment")]
        public IActionResult GetEquipment()
        {
            return Ok(_gameControllerService.GetEquipment());
        }
        [HttpPost("inventory/{itemId}/equip")]
        public IActionResult EquipInventoryItem(int itemId)
        {
            _gameControllerService.EquipInventoryItem(itemId);
            return GetEquipment();
        }
        [HttpPost("equipment/weapon/unequip")]
        public IActionResult UnequipWeapon()
        {
            _gameControllerService.UnequipWeapon();
            return GetEquipment();
        }
        [HttpPost("equipment/helm/unequip")]
        public IActionResult UnequipHelm()
        {
            _gameControllerService.UnequipHelm();
            return GetEquipment();
        }
        [HttpPost("equipment/chestplate/unequip")]
        public IActionResult UnequipChestplate()
        {
            _gameControllerService.UnequipChestplate();
            return GetEquipment();
        }
    }
}