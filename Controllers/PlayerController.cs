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
            var room = gameRepository.ShowCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }

        [HttpGet("inventory")]
        public IResult ShowInventory()
        {
            var inventory = gameRepository.ShowInventory();
            return Results.Ok(new SuccessfulResponse(inventory));
        }

        [HttpGet("coins")]
        public IResult ShowCoins()
        {
            var coins = gameRepository.ShowCoins();
            return Results.Ok(new SuccessfulResponse(coins));
        }
    }
}
