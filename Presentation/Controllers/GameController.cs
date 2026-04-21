using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Mappers;

namespace TextGame.Presentation.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController
    {
        private readonly IGameControllerService _gameControllerService;

        public GameController(IGameControllerService gameControllerRepository)
        {
            _gameControllerService = gameControllerRepository;
        }

        [HttpPost("start")]
        public IResult Start()
        {
            _gameControllerService.Start();
            //return Results.Ok(new SuccessfulResponse("Игра успешно начата."));
            var room = _gameControllerService.GetCurrentRoom();
            return Results.Ok(GameObjectMapper.ToDTO(room));
        }
        [HttpGet("info")]
        public IResult GetInfo()
        {
            return Results.Ok(_gameControllerService.GetGameInfo());
        }
        [HttpGet("map")]
        public IResult GetMap()
        {
            return Results.Ok(_gameControllerService.GetMap());
        }
        [HttpGet("coins")]
        public IResult GetCoins()
        {
            return Results.Ok(_gameControllerService.GetCoins());
        }
        [HttpGet("keys")]
        public IResult GetKeys()
        {
            return Results.Ok(_gameControllerService.GetKeys());
        }
        [HttpGet("inventory")]
        public IResult GetInventory()
        {
            return Results.Ok(_gameControllerService.GetInventory());
        }
        [HttpGet("inventory/{itemId}")]
        public IResult GetInventoryItem(int itemId)
        {
            return Results.Ok(_gameControllerService.GetInventoryItem(itemId));
        }
        [HttpPost("inventory/{itemId}/sell")]
        public IResult SellInventoryItem(int itemId)
        {
            _gameControllerService.SellInventoryItem(itemId);
            return GetInfo();
        }
        [HttpPost("inventory/{itemId}/use")]
        public IResult UseInventoryItem(int itemId)
        {
            _gameControllerService.UseInventoryItem(itemId);
            return GetInfo();
        }
        [HttpGet("equipment")]
        public IResult GetEquipment()
        {
            return Results.Ok(_gameControllerService.GetEquipment());
        }
        [HttpPost("inventory/{itemId}/equip")]
        public IResult EquipInventoryItem(int itemId)
        {
            _gameControllerService.EquipInventoryItem(itemId);
            return GetEquipment();
        }
        [HttpPost("equipment/weapon/unequip")]
        public IResult UnequipWeapon()
        {
            _gameControllerService.UnequipWeapon();
            return GetEquipment();
        }
        [HttpPost("equipment/helm/unequip")]
        public IResult UnequipHelm()
        {
            _gameControllerService.UnequipHelm();
            return GetEquipment();
        }
        [HttpPost("equipment/chestplate/unequip")]
        public IResult UnequipChestplate()
        {
            _gameControllerService.UnequipChestplate();
            return GetEquipment();
        }
    }
}