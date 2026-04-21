using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.DTO;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Presentation.Mappers;

namespace TextGame.Presentation.Controllers
{

    [ApiController]
    [Route("rooms")]
    public class RoomController :ControllerBase
    {
        private readonly IRoomControllerService _roomControllerService;

        public RoomController(IRoomControllerService roomControllerService)
        {
            _roomControllerService = roomControllerService;
        }
        [HttpPost("next")]
        public IActionResult GoNextRoom()
        {
            var room = _roomControllerService.GoNextRoom();
            return Ok(GameObjectMapper.ToDTO(room));
        }
        [HttpPost("{roomId}")]
        public IActionResult GoRoom(int roomId)
        {
            var room = _roomControllerService.GoToRoom(roomId);
            return Ok(GameObjectMapper.ToDTO(room));
        }
        [HttpGet("current")]
        public IActionResult GetCurrentRoom()
        {
            var room = _roomControllerService.GetCurrentRoom();
            return Ok(GameObjectMapper.ToDTO(room));
        }
        [HttpPost("current/items")]
        public IActionResult Search()
        {
            var items = _roomControllerService.Search();
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Ok(itemsDTOs);
        }
        [HttpPost("current/items/{itemId}/take")]
        public IActionResult TakeItem(int itemId)
        {
            _roomControllerService.TakeItem(itemId);
            return Ok(_roomControllerService.GetGameInfo());
        }
        [HttpPost("current/items/takeall")]
        public IActionResult TakeAllItems()
        {
            _roomControllerService.TakeAllItems();
            return Ok(_roomControllerService.GetGameInfo());
        }
        [HttpPost("current/items/{itemId}/buy")]
        public IActionResult BuyItem(int itemId)
        {
            _roomControllerService.BuyItem(itemId);
            return Ok(_roomControllerService.GetGameInfo());
        }
        #region CHEST

        [HttpPost("current/items/{chestId}/chest/hit")]
        public IActionResult HitChest(int chestId)
        {
            return Ok(_roomControllerService.HitChest(chestId));
        }
        [HttpPost("current/items/{chestId}/chest/open")]
        public IActionResult OpenChest(int chestId)
        {
            _roomControllerService.OpenChest(chestId);
            return SearchChest(chestId);
        }
        [HttpPost("current/items/{chestId}/chest/unlock")]
        public IActionResult UnlockChest(int chestId)
        {
            var chest = _roomControllerService.UnlockChest(chestId);
            return Ok(GameObjectMapper.ToDTO(chest));
        }
        [HttpPost("current/items/{chestId}/chest/items")]
        public IActionResult SearchChest(int chestId)
        {
            var items = _roomControllerService.SearchChest(chestId);
            var itemsDTOs = items.Select(GameObjectMapper.ToDTO).ToList();
            return Ok(itemsDTOs);
        }
        [HttpPost("current/items/{chestId}/chest/items/{itemId}/take")]
        public IActionResult TakeItemFromChest(int chestId, int itemId)
        {
            _roomControllerService.TakeItemFromChest(chestId, itemId);
            return Ok(_roomControllerService.GetGameInfo());
        }
        [HttpPost("current/items/{chestId}/chest/items/takeall")]
        public IActionResult TakeAllItemsFromChest(int chestId)
        {
            _roomControllerService.TakeAllItemsFromChest(chestId);
            return Ok(_roomControllerService.GetGameInfo());
        }
        #endregion
        #region ENEMIES
        //[HttpGet("current/enemy/{enemyId}")]
        //public IResult GetEnemy(int enemyId)
        //{
        //    Enemy enemy = RoomControllerRepository.GetEnemy(enemyId);
        //    return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(enemy)));
        //}
        //[HttpGet("current/enemies")]
        //public IResult GetEnemies()
        //{
        //    List<Enemy> enemies = RoomControllerRepository.GetEnemies();
        //    return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(enemies)));
        //}
        [HttpGet("current/enemy")]
        public IActionResult GetEnemy()
        {
            Enemy enemy = _roomControllerService.GetEnemy();
            return Ok(GameObjectMapper.ToDTO(enemy));
        }
        //[HttpPost("current/enemy/{enemyId}/attack")]
        [HttpPost("current/enemy/attack")]
        public IActionResult AttackEnemy()
        {
            List<BattleLog> battleLogs = [
                _roomControllerService.DealDamage(),
                _roomControllerService.GetDamage()];
            return Ok(battleLogs);
        }
        #endregion
    }
}