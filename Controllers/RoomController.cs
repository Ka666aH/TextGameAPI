//TODO
//Исправить статус коды
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
                var room = gameRepository.ShowCurrentRoom();
                return Results.Ok(new SuccessfulResponse(room));
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
        [HttpGet("{roomId}/items")]
        public IResult Search(int roomId)
        {
            try
            {
                var items = gameRepository.Search(roomId);
                return Results.Ok(new SuccessfulResponse(items));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return Results.NotFound(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.SearchError, ex.Message), statusCode: 500);
            }
        }
        [HttpPost("{roomId}/items/take/{itemId}")]
        public IResult TakeItem(int roomId, int itemId)
        {
            try
            {
                gameRepository.TakeItem(roomId, itemId);
                return Results.Ok(new SuccessfulResponse(gameRepository.ShowInventoryItem(itemId)));

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
        [HttpPost("{roomId}/items/takeall")]
        public IResult TakeAllItems(int roomId)
        {
            try
            {
                var itemsIds = gameRepository.Search(roomId).Where(i => i.IsCarryable == true).Select(i => i.Id).ToList();
                gameRepository.TakeAllItems(roomId);
                return Results.Ok(new SuccessfulResponse(gameRepository.ShowInventoryItems(itemsIds)));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (EmptyException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.EmptyError, ex.Message));
            }
            catch (ArgumentNullException ex)
            {
                return Results.NotFound(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
            }
        }
        #region CHEST
        //[HttpGet("{roomId}/items/{chestId}/chest/islocked")]
        //public IResult CheckState(int chestId) //упразднить
        //{
        //    try
        //    {
        //        return Results.Ok(new SuccessfulResponse(gameRepository.CheckChest(chestId))); //вернуть сундук и свойство
        //    }
        //    catch (UnstartedGameException ex)
        //    {
        //        return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
        //    }
        //    catch (ArgumentNullException ex)
        //    {
        //        return Results.BadRequest(new ErrorResponse(ErrorCodes.NotFound, ex.Message));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return Results.BadRequest(new ErrorResponse(ErrorCodes.NotChestError, ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return Results.Json(new ErrorResponse(ErrorCodes.TakeItemsError, ex.Message), statusCode: 500);
        //    }
        //}
        [HttpPost("{roomId}/items/{chestId}/chest/open")]
        public IResult OpenChest(int roomId, int chestId)
        {
            try
            {
                gameRepository.OpenChest(roomId, chestId);
                return Results.Ok(new SuccessfulResponse(gameRepository.SearchChest(roomId, chestId)));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (LockedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
            }
            catch (ClosedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
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
        [HttpPost("{roomId}/items/{chestId}/chest/unlock")]
        public IResult UnlockChest(int roomId, int chestId)
        {
            try
            {
                gameRepository.UnlockChest(roomId, chestId);
                return Results.Ok(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (NoKeyException ex)
            {
                return Results.Json(new ErrorResponse(ErrorCodes.NoKeyError, ex.Message), statusCode: 403);
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
        [HttpGet("{roomId}/items/{chestId}/chest/search")]
        public IResult SearchChest(int roomId, int chestId)
        {
            try
            {
                return Results.Ok(new SuccessfulResponse(gameRepository.SearchChest(roomId, chestId)));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (LockedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
            }
            catch (ClosedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
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
        [HttpPost("{roomId}/items/{chestId}/chest/take/{itemId}")]
        public IResult TakeItemFromChest(int roomId, int chestId, int itemId)
        {
            try
            {
                gameRepository.TakeItemFromChest(roomId, chestId, itemId);
                return Results.Ok(new SuccessfulResponse(gameRepository.ShowInventoryItem(itemId)));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (LockedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
            }
            catch (ClosedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
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
        [HttpPost("{roomId}/items/{chestId}/chest/takeall")]
        public IResult TakeAllItemsFromChest(int roomId, int chestId)
        {
            try
            {
                var itemsIds = gameRepository.SearchChest(roomId, chestId).Where(i => i.IsCarryable == true).Select(i => i.Id).ToList();
                gameRepository.TakeAllItemsFromChest(roomId, chestId);
                return Results.Ok(new SuccessfulResponse(gameRepository.ShowInventoryItems(itemsIds)));
            }
            catch (UnstartedGameException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.UnstartedGameError, ex.Message));
            }
            catch (EmptyException ex)
            {
                return Results.BadRequest(new ErrorResponse(ErrorCodes.EmptyError, ex.Message));
            }
            catch (LockedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
            }
            catch (ClosedException)
            {
                return Results.BadRequest(new SuccessfulResponse(gameRepository.ReturnChestDTO(roomId, chestId)));
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
