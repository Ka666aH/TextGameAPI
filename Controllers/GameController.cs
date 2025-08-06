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
            return Results.Ok(new SuccessfulResponse("Игра успешно начата."));
        }
    }
}