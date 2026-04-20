using Microsoft.AspNetCore.Mvc;
using TextGame.Application.Interfaces.Services;
using TextGame.Domain.DTO;
using TextGame.Domain.GameObjects.Enemies;
using TextGame.Presentation.Mappers;
using TextGame.Presentation.ResponseTemplates;
namespace TextGame.Presentation.Controllers
{

    [ApiController]
    [Route("rooms")]
    public class RoomController
    {
        private readonly IRoomControllerService _roomControllerService;

        public RoomController(IRoomControllerService roomControllerService)
        {
            _roomControllerService = roomControllerService;
        }
        [HttpPost("next")]
        public IResult GoNextRoom()
        {
            var room = _roomControllerService.GoNextRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpPost("{roomId}")]
        public IResult GoRoom(int roomId)
        {
            var room = _roomControllerService.GoToRoom(roomId);
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpGet("current")]
        public IResult GetCurrentRoom()
        {
            var room = _roomControllerService.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpPost("current/items")]
        public IResult Search()
        {
            var items = _roomControllerService.Search();
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("current/items/{itemId}/take")]
        public IResult TakeItem(int itemId)
        {
            _roomControllerService.TakeItem(itemId);
            return Results.Ok(new SuccessfulResponse(_roomControllerService.GetGameInfo()));
        }
        [HttpPost("current/items/takeall")]
        public IResult TakeAllItems()
        {
            _roomControllerService.TakeAllItems();
            return Results.Ok(new SuccessfulResponse(_roomControllerService.GetGameInfo()));
        }
        [HttpPost("current/items/{itemId}/buy")]
        public IResult BuyItem(int itemId)
        {
            _roomControllerService.BuyItem(itemId);
            return Results.Ok(new SuccessfulResponse(_roomControllerService.GetGameInfo()));
        }
        #region CHEST

        [HttpPost("current/items/{chestId}/chest/hit")]
        public IResult HitChest(int chestId)
        {
            return Results.Ok(new SuccessfulResponse(_roomControllerService.HitChest(chestId)));
        }
        [HttpPost("current/items/{chestId}/chest/open")]
        public IResult OpenChest(int chestId)
        {
            _roomControllerService.OpenChest(chestId);
            return SearchChest(chestId);
        }
        [HttpPost("current/items/{chestId}/chest/unlock")]
        public IResult UnlockChest(int chestId)
        {
            var chest = _roomControllerService.UnlockChest(chestId);
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(chest)));
        }
        [HttpPost("current/items/{chestId}/chest/items")]
        public IResult SearchChest(int chestId)
        {
            var items = _roomControllerService.SearchChest(chestId);
            var itemsDTOs = items.Select(GameObjectMapper.ToDTO).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("current/items/{chestId}/chest/items/{itemId}/take")]
        public IResult TakeItemFromChest(int chestId, int itemId)
        {
            _roomControllerService.TakeItemFromChest(chestId, itemId);
            return Results.Ok(new SuccessfulResponse(_roomControllerService.GetGameInfo()));
        }
        [HttpPost("current/items/{chestId}/chest/items/takeall")]
        public IResult TakeAllItemsFromChest(int chestId)
        {
            _roomControllerService.TakeAllItemsFromChest(chestId);
            return Results.Ok(new SuccessfulResponse(_roomControllerService.GetGameInfo()));
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
        public IResult GetEnemy()
        {
            Enemy enemy = _roomControllerService.GetEnemy();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(enemy)));
        }
        //[HttpPost("current/enemy/{enemyId}/attack")]
        [HttpPost("current/enemy/attack")]
        public IResult AttackEnemy()
        {
            List<BattleLog> battleLogs = new List<BattleLog>();
            battleLogs.Add(_roomControllerService.DealDamage());
            battleLogs.Add(_roomControllerService.GetDamage());
            return Results.Ok(new SuccessfulResponse(battleLogs));
        }
        #endregion
    }
}