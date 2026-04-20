using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.ResponseTemplates;

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
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpGet("info")]
        public IResult GetInfo()
        {
            var info = _gameControllerService.GetGameInfo();
            return Results.Ok(new SuccessfulResponse(info));
        }
        [HttpGet("map")]
        public IResult GetMap()
        {
            return Results.Ok(new SuccessfulResponse(_gameControllerService.GetMap()));
        }
        [HttpGet("coins")]
        public IResult GetCoins()
        {
            var coins = _gameControllerService.GetCoins();
            return Results.Ok(new SuccessfulResponse(coins));
        }
        [HttpGet("keys")]
        public IResult GetKeys()
        {
            var keys = _gameControllerService.GetKeys();
            return Results.Ok(new SuccessfulResponse(keys));
        }
        [HttpGet("inventory")]
        public IResult GetInventory()
        {
            var inventory = _gameControllerService.GetInventory();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(inventory)));
        }
        [HttpGet("inventory/{itemId}")]
        public IResult GetInventoryItem(int itemId)
        {
            var item = _gameControllerService.GetInventoryItem(itemId);
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(item)));
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
        [HttpPost("inventory/{itemId}/equip")]
        public IResult EquipInventoryItem(int itemId)
        {
            _gameControllerService.EquipInventoryItem(itemId);
            var equipment = _gameControllerService.GetEquipment();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpGet("equipment")]
        public IResult GetEquipment()
        {
            var equipment = _gameControllerService.GetEquipment();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpPost("equipment/weapon/unequip")]
        public IResult UnequipWeapon()
        {
            _gameControllerService.UnequipWeapon();
            var equipment = _gameControllerService.GetEquipment();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpPost("equipment/helm/unequip")]
        public IResult UnequipHelm()
        {
            _gameControllerService.UnequipHelm();
            var equipment = _gameControllerService.GetEquipment();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpPost("equipment/chestplate/unequip")]
        public IResult UnequipChestplate()
        {
            _gameControllerService.UnequipChestplate();
            var equipment = _gameControllerService.GetEquipment();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
    }
}