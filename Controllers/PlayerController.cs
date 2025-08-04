using Microsoft.AspNetCore.Mvc;
namespace TextGame.Controllers
{
    [ApiController]
    [Route("player")]
    public class PlayerController
    {
        private readonly IGameRepository gameRepository;

        public PlayerController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }

        [HttpGet("currentroom")]
        public IResult ShowCurrentRoom()
        {
            try
            {
                var room = gameRepository.ShowCurrentRoom();
                return Results.Ok(room);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при получении текущей комнаты: {ex.Message}");
            }
        }

        [HttpGet("inventory")]
        public IResult ShowInventory()
        {
            try
            {
                var inventory = gameRepository.ShowInventory();
                return Results.Ok(inventory);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при получении инвентаря: {ex.Message}");
            }
        }

        [HttpGet("coins")]
        public IResult ShowCoins()
        {
            try
            {
                var coins = gameRepository.ShowCoins();
                return Results.Ok(coins);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка при получении количества монет: {ex.Message}");
            }
        }
    }
}
