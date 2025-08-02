using Microsoft.AspNetCore.Mvc;
using TextGame;

namespace TextGame.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController
    {
        private readonly IGameRepository gameRepository;

        public GameController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }

        [HttpPost("start")]
        public IResult Start()
        {
            try
            {
                gameRepository.Start();
                return Results.Ok("Игра успешно начата.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Игра не началась из-за ошибки.\n{ex.Message}.");
            }
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