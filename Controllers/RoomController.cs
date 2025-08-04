using Microsoft.AspNetCore.Mvc;
namespace TextGame.Controllers
{
    [ApiController]
    [Route("room")]
    public class RoomController
    {
        private readonly IGameRepository gameRepository;

        public RoomController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        [HttpPost("next")]
        public IResult GoNextRoom()
        {
            try
            {
                gameRepository.GoNextRoom();
                return Results.Ok("Переход в следующую комнату выполнен.");
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при переходе в следующую комнату: {ex.Message}");
            }
        }

        //[HttpGet("{roomId}/items")]
        [HttpGet("search")]
        public IResult Search()
        {
            try
            {
                var items = gameRepository.Search();
                return Results.Ok(items);
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при поиске предметов: {ex.Message}");
            }
        }
        [HttpPost("items/take/{itemId}")]
        public IResult TakeItem(int itemId)
        {
            try
            {
                gameRepository.TakeItem(itemId);
                return Results.Ok("Предмет получен.");

            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (UncarryableException ex)
            {
                return Results.Ok($"{ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest($"{ex.ParamName}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при подъёме предмета: {ex.Message}");
            }
        }
        [HttpPost("items/takeall")]
        public IResult TakeAllItems()
        {
            try
            {
                gameRepository.TakeAllItems();
                return Results.Ok("Предметы получены.");

            }
            catch (EmptyException ex)
            {
                return Results.Ok(ex.Message);
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при подъёме предмета: {ex.Message}");
            }
        }
        //CHEST
        [HttpGet("chest/{chestId}/islocked")]
        public IResult CheckState(int chestId)
        {
            try
            {
                return Results.Ok(gameRepository.CheckChest(chestId));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"{ex.Message}");
            }
        }
        [HttpPost("chest/{chestId}/unlock")]
        public IResult UnlockChest(int chestId)
        {
            try
            {
                return Results.Ok(gameRepository.UnlockChest(chestId));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"{ex.Message}");
            }
        }
        [HttpPost("chest/{chestId}/open")]
        public IResult OpenChest(int id)
        {
            try
            {
                return Results.Ok(gameRepository.OpenChest(id));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (LockedException ex)
            {
                return Results.Ok($"{ex.Message}");
            }
            catch (ClosedException ex)
            {
                return Results.Ok($"{ex.Message}");
            }
            catch (MimicException ex)
            {
                return Results.Ok($"{ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"{ex.Message}");
            }
        }
        [HttpPost("chest/{chestId}/take/{itemId}")]
        public IResult TakeItemFromChest(int chestId, int itemId)
        {
            try
            {
                gameRepository.TakeItemFromChest(chestId, itemId);
                return Results.Ok("Предмет получен.");
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (LockedException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (ClosedException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"{ex.Message}");
            }
        }
        [HttpPost("chest/{chestId}/takeall")]
        public IResult TakeAllItemsFromChest(int chestId)
        {
            try
            {
                gameRepository.TakeAllItemsFromChest(chestId);
                return Results.Ok("Предметы получены.");
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (EmptyException ex)
            {
                return Results.Ok(ex.Message);
            }
            catch (LockedException ex)
            {
                return Results.Ok($"{ex.Message}");
            }
            catch (ClosedException ex)
            {
                return Results.Ok($"{ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return Results.Problem($"{ex.Message}");
            }
        }
    }
}
