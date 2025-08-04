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
                return Results.Ok(new SuccessfulResponse("Игра успешно начата."));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message),statusCode: 500);
            }
        }
    }
}