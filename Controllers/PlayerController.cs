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
                return Results.Ok(new SuccessfulResponse(room));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.CurrentRoomError, ex.Message),statusCode: 500);
            }
        }

        [HttpGet("inventory")]
        public IResult ShowInventory()
        {
            try
            {
                var inventory = gameRepository.ShowInventory();
                return Results.Ok(new SuccessfulResponse(inventory));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.InventoryError, ex.Message), statusCode: 500);
            }
        }

        [HttpGet("coins")]
        public IResult ShowCoins()
        {
            try
            {
                var coins = gameRepository.ShowCoins();
                return Results.Ok(new SuccessfulResponse(coins));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.CoinsError, ex.Message), statusCode: 500);
            }
        }
    }
}
