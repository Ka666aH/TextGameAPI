using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("")]
        public IResult Start()
        {
            gameRepository.Start();
            //return Results.Ok(new SuccessfulResponse("Игра успешно начата."));
            var room = gameRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }
        [HttpGet("map")]
        public IResult GetMap()
        {
            return Results.Ok(new SuccessfulResponse(gameRepository.GetMap()));
        }
        [HttpGet("currentroom")]
        public IResult GetCurrentRoom()
        {
            var room = gameRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }

        [HttpGet("inventory")]
        public IResult GetInventory()
        {
            var inventory = gameRepository.GetInventory();
            return Results.Ok(new SuccessfulResponse(inventory));
        }

        [HttpGet("coins")]
        public IResult GetCoins()
        {
            var coins = gameRepository.GetCoins();
            return Results.Ok(new SuccessfulResponse(coins));
        }
    }
}