using Microsoft.AspNetCore.Mvc;
namespace TextGame.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController
    {
        private readonly IGameControllerRepository GameControllerRepository;

        public GameController(IGameControllerRepository gameControllerRepository)
        {
            GameControllerRepository = gameControllerRepository;
        }

        [HttpPost("start")]
        public IResult Start()
        {
            GameControllerRepository.Start();
            //return Results.Ok(new SuccessfulResponse("Игра успешно начата."));
            var room = GameControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpGet("info")]
        public IResult GetInfo()
        {
            var info = GameControllerRepository.GetGameInfo();
            return Results.Ok(new SuccessfulResponse(info));
        }
        [HttpGet("map")]
        public IResult GetMap()
        {
            return Results.Ok(new SuccessfulResponse(GameControllerRepository.GetMap()));
        }
        [HttpGet("coins")]
        public IResult GetCoins()
        {
            var coins = GameControllerRepository.GetCoins();
            return Results.Ok(new SuccessfulResponse(coins));
        }
        [HttpGet("keys")]
        public IResult GetKeys()
        {
            var keys = GameControllerRepository.GetKeys();
            return Results.Ok(new SuccessfulResponse(keys));
        }
        [HttpGet("inventory")]
        public IResult GetInventory()
        {
            var inventory = GameControllerRepository.GetInventory();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(inventory)));
        }
        [HttpGet("inventory/{itemId}")]
        public IResult GetInventoryItem(int itemId)
        {
            var item = GameControllerRepository.GetInventoryItem(itemId);
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(item)));
        }
        [HttpPost("inventory/{itemId}/sell")]
        public IResult SellInventoryItem(int itemId)
        {
            GameControllerRepository.SellInventoryItem(itemId);
            return GetInfo();
        }
        [HttpPost("inventory/{itemId}/use")]
        public IResult UseInventoryItem(int itemId)
        {
            GameControllerRepository.UseInventoryItem(itemId);
            return GetInfo();
        }
        [HttpPost("inventory/{itemId}/equip")]
        public IResult EquipInventoryItem(int itemId)
        {
            var equipment = GameControllerRepository.EquipInventoryItem(itemId);
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpGet("equipment")]
        public IResult GetEquipment()
        {
            var equipment = GameControllerRepository.GetEquipment();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpPost("equipment/weapon/unequip")]
        public IResult UnequipWeapon()
        {
            var equipment = GameControllerRepository.UnequipWeapon();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpPost("equipment/helm/unequip")]
        public IResult UnequipHelm()
        {
            var equipment = GameControllerRepository.UnequipHelm();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
        [HttpPost("equipment/chestplate/unequip")]
        public IResult UnequipChestplate()
        {
            var equipment = GameControllerRepository.UnequipChestplate();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(equipment)));
        }
    }
}