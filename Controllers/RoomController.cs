using Microsoft.AspNetCore.Mvc;
using System.Drawing;
namespace TextGame.Controllers
{

    [ApiController]
    [Route("room")]
    public class RoomController
    {
        private static readonly string itemsReceived = "Предметы получены.";
        private static readonly string itemReceived = "Предмет получен.";

        private readonly IGameRepository gameRepository;

        public RoomController(IGameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        [HttpPost("next")]
        public IResult GoNextRoom()
        {
            try
            {
                gameRepository.GoNextRoom();
                return Results.Ok(new SuccessfulResponse("Переход в следующую комнату выполнен.")); //выдавать комнату
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.NextRoomError, ex.Message), statusCode: 500);
            }
        }

        //[HttpGet("{roomId}/items")]
        [HttpGet("search")]
        public IResult Search()
        {
            try
            {
                var items = gameRepository.Search();
                return Results.Ok(new SuccessfulResponse(items));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.SearchError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("items/take/{itemId}")]
        public IResult TakeItem(int itemId)
        {
            try
            {
                gameRepository.TakeItem(itemId);
                return Results.Ok(new SuccessfulResponse(itemReceived)); //вернуть предмет из инвентаря

            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (UncarryableException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UncarryableError, ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return Results.NotFound(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("items/takeall")]
        public IResult TakeAllItems()
        {
            try
            {
                gameRepository.TakeAllItems();
                return Results.Ok(new SuccessfulResponse(itemsReceived)); //вернуть предметы из инвентаря?
            }
            catch (EmptyException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.EmptyError, ex.Message));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        #region CHEST
        [HttpGet("chest/{chestId}/islocked")]
        public IResult CheckState(int chestId) //упразднить
        {
            try
            {
                return Results.Ok(new SuccessfulResponse(gameRepository.CheckChest(chestId))); //вернуть сундук и свойство
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotChestError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("chest/{chestId}/unlock")]
        public IResult UnlockChest(int chestId)
        {
            try
            {
                return Results.Ok(new SuccessfulResponse(gameRepository.UnlockChest(chestId))); //вернуть сундук и свойство
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotChestError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("chest/{chestId}/open")]
        public IResult OpenChest(int id)
        {
            try
            {
                return Results.Ok(new SuccessfulResponse(gameRepository.OpenChest(id))); //разделить открытие и лут, и объдинить их (не все поймут)
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (LockedException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message)); //вернуть сундук и свойство? 
            }
            catch (ClosedException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message)); //вернуть сундук и свойство?
            }
            catch (MimicException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotChestError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("chest/{chestId}/take/{itemId}")]
        public IResult TakeItemFromChest(int chestId, int itemId)
        {
            try
            {
                gameRepository.TakeItemFromChest(chestId, itemId);
                return Results.Ok(new SuccessfulResponse(itemReceived)); //вернуть предмет из инвентаря
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (LockedException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message)); //вернуть сундук и свойство? 
            }
            catch (ClosedException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message)); //вернуть сундук и свойство?
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotChestError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("chest/{chestId}/takeall")]
        public IResult TakeAllItemsFromChest(int chestId)
        {
            try
            {
                gameRepository.TakeAllItemsFromChest(chestId);
                return Results.Ok(new SuccessfulResponse(itemsReceived)); //вернуть предметы из инвентаря?
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (EmptyException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.EmptyError, ex.Message));
            }
            catch (LockedException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message)); //вернуть сундук и свойство? 
            }
            catch (ClosedException ex)
            {
                return Results.Ok(new SuccessfulResponse(ex.Message)); //вернуть сундук и свойство?
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.NotChestError, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        #endregion
    }
}
