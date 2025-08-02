using Microsoft.AspNetCore.Mvc;
using TextGame;
//разделить OPEN и SEARCH
namespace TextGame.Controllers
{
    [ApiController]
    [Route("chest")]
    public class ChestController
    {
        private readonly IGameRepository gameRepository;
        //public readonly Chest chest;

        public ChestController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        [HttpGet("isclosed/{id}")]
        public IResult CheckState(int id)
        {
            try
            {
                return Results.Ok(gameRepository.CheckChest(id));
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
        [HttpPost("unlock/{id}")]
        public IResult UnlockChest(int id)
        {
            try
            {
                return Results.Ok(gameRepository.UnlockChest(id));
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
        [HttpPost("open/{id}")]
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
        [HttpPost("takefromchest/{chestId}/{itemId}")]
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
        [HttpPost("takefromchest/{chestId}")]
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
