using Microsoft.AspNetCore.Mvc;

namespace TextGame.Controllers
{
    [ApiController]
    [Route("action")]
    public class ActionController
    {
        private readonly IGameRepository gameRepository;

        public ActionController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        [HttpPost("nextroom")]
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
        [HttpPost("take/{id}")]
        public IResult TakeItem(int id)
        {
            try
            {
                gameRepository.TakeItem(id);
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
        [HttpPost("take")]
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
    }
}
