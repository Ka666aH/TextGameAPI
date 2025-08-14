using Microsoft.AspNetCore.Mvc;
using System.Drawing;
namespace TextGame.Controllers
{

    [ApiController]
    [Route("room")]
    public class RoomController
    {
        private readonly IRoomControllerRepository RoomControllerRepository;

        public RoomController(IRoomControllerRepository roomControllerRepository)
        {
            RoomControllerRepository = roomControllerRepository;
        }
        [HttpPost("next")]
        public IResult GoNextRoom()
        {
            RoomControllerRepository.GoNextRoom();
            var room = RoomControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpPost("{roomId}")]
        public IResult GoRoom(int roomId)
        {
            RoomControllerRepository.GetRoomById(roomId);
            var room = RoomControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(room)));
        }
        [HttpPost("current/items")]
        public IResult Search()
        {
            var items = RoomControllerRepository.Search();
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("current/items/{itemId}/take")]
        public IResult TakeItem(int itemId)
        {
            RoomControllerRepository.TakeItem(itemId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameInfo()));
        }
        [HttpPost("current/items/takeall")]
        public IResult TakeAllItems()
        {
            RoomControllerRepository.TakeAllItems();
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameInfo()));
        }
        #region CHEST
        [HttpPost("current/items/{chestId}/chest/hit")]
        public IResult HitChest(int chestId)
        {
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.HitChest(chestId)));
        }
        [HttpPost("current/items/{chestId}/chest/open")]
        public IResult OpenChest(int chestId)
        {
            RoomControllerRepository.OpenChest(chestId);
            var items = RoomControllerRepository.SearchChest(chestId);
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("current/items/{chestId}/chest/unlock")]
        public IResult UnlockChest(int chestId)
        {
            RoomControllerRepository.UnlockChest(chestId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.ReturnChestDTO(chestId)));
        }
        [HttpPost("current/items/{chestId}/chest/items")]
        public IResult SearchChest(int chestId)
        {
            var items = RoomControllerRepository.SearchChest(chestId);
            var itemsDTOs = items.Select(item => GameObjectMapper.ToDTO(item)).ToList();
            return Results.Ok(new SuccessfulResponse(itemsDTOs));
        }
        [HttpPost("current/items/{chestId}/chest/items/{itemId}/take")]
        public IResult TakeItemFromChest(int chestId, int itemId)
        {
            RoomControllerRepository.TakeItemFromChest(chestId, itemId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameInfo()));
        }
        [HttpPost("current/items/{chestId}/chest/items/takeall")]
        public IResult TakeAllItemsFromChest(int chestId)
        {
            RoomControllerRepository.TakeAllItemsFromChest(chestId);
            return Results.Ok(new SuccessfulResponse(RoomControllerRepository.GetGameInfo()));
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
            Enemy enemy = RoomControllerRepository.GetEnemyById();
            return Results.Ok(new SuccessfulResponse(GameObjectMapper.ToDTO(enemy)));
        }
        //[HttpPost("current/enemy/{enemyId}/attack")]
        [HttpPost("current/enemy/attack")]
        public IResult AttackEnemy()
        {
            List<BattleLog> battleLogs = new List<BattleLog>();
            battleLogs.Add(RoomControllerRepository.DealDamage());
            battleLogs.Add(RoomControllerRepository.GetDamage());
            return Results.Ok(new SuccessfulResponse(battleLogs));
        }
        #endregion
    }
}