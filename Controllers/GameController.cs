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

        

    }
}