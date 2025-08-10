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
        [HttpGet("map")]
        public IResult GetMap()
        {
            return Results.Ok(new SuccessfulResponse(GameControllerRepository.GetMap()));
        }
        [HttpGet("currentroom")]
        public IResult GetCurrentRoom()
        {
            var room = GameControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
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
            var inventoryDTOs = inventory.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(inventoryDTOs));
        }
        [HttpGet("inventory/{itemId}")]
        public IResult GetInventoryItem(int itemId)
        {
            var item = GameControllerRepository.GetInventoryItem(itemId);
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(item)));
        }
        [HttpGet("equipment")]
        public IResult GetEquipment()
        {
            var equipmentDTO = GameControllerRepository.GetEquipment();
            return Results.Ok(new SuccessfulResponse(equipmentDTO));
        }
        [HttpPost("equipment/{itemId}/equip")]
        public IResult EquipInventoryItem(int itemId)
        {
            var equipmentDTO = GameControllerRepository.EquipInventoryItem(itemId);
            return Results.Ok(new SuccessfulResponse(equipmentDTO));
        }
        [HttpPost("equipment/weapon/unequip")]
        public IResult UnequipWeapon()
        {
            var equipmentDTO = GameControllerRepository.UnequipWeapon();
            return Results.Ok(new SuccessfulResponse(equipmentDTO));
        }
        [HttpPost("equipment/helm/unequip")]
        public IResult UnequipHelm()
        {
            var equipmentDTO = GameControllerRepository.UnequipHelm();
            return Results.Ok(new SuccessfulResponse(equipmentDTO));
        }
        [HttpPost("equipment/chestplate/unequip")]
        public IResult UnequipChestplate()
        {
            var equipmentDTO = GameControllerRepository.UnequipChestplate();
            return Results.Ok(new SuccessfulResponse(equipmentDTO));
        }
    }
}