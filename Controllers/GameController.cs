using Microsoft.AspNetCore.Mvc;
namespace TextGame.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController
    {
        private readonly IGameControllerRepository GameControllerRepository;

        public GameController(IGameControllerRepository gameControllerRepository)
        {
            GameControllerRepository = gameControllerRepository;
        }

        [HttpPost("")]
        public IResult Start()
        {
            GameControllerRepository.Start();
            //return Results.Ok(new SuccessfulResponse("Игра успешно начата."));
            var room = GameControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }
        [HttpGet("map")]
        public IResult GetMap()
        {
            return Results.Ok(new SuccessfulResponse(GameControllerRepository.GetMap()));
        }
        [HttpGet("currentroom")]
        public IResult GetCurrentRoom()
        {
            var room = GameControllerRepository.GetCurrentRoom();
            return Results.Ok(new SuccessfulResponse(room));
        }
        [HttpGet("coins")]
        public IResult GetCoins()
        {
            var coins = GameControllerRepository.GetCoins();
            return Results.Ok(new SuccessfulResponse(coins));
        }
        [HttpGet("keys")]
        public IResult GetKeys()
        {
            var keys = GameControllerRepository.GetKeys();
            return Results.Ok(new SuccessfulResponse(keys));
        }
        [HttpGet("inventory")]
        public IResult GetInventory()
        {
            var inventory = GameControllerRepository.GetInventory();
            return Results.Ok(new SuccessfulResponse(inventory));
        }
    }
}